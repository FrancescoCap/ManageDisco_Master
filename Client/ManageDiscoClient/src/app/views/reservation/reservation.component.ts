import { ViewContainerRef } from '@angular/core';
import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { forkJoin } from 'rxjs/internal/observable/forkJoin';
import { catchError} from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalModelList, ModalTextBoxList, ModalViewGroup } from '../../components/modal/modal.model';
import { EventParty, ModalType, PrCustomerView, Reservation, ReservationPost, ReservationType } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-reservation',
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.scss', '../../app.component.scss']
})
export class ReservationComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  isLoading = false;
  openNewResModal = false;
  newResModaltitle = "Nuova prenotazione";
  editBudget: boolean = false;
 
  reservations?: Reservation[];
  events?: EventParty[];
  reservationTypes?: ReservationType[];
  newReservation: ReservationPost = { eventPartyId: 0, reservationPeopleCount: 0, reservationExpectedBudget: 0, reservationUserCodeValue: "" };
  prCustomers?: PrCustomerView[];

  modalModelLists: ModalModelList[] = [];
  modalTextBoxLists: ModalTextBoxList[] = [
    { id: 'reservationName', label: 'Nome prenotazione' },
    { id: 'reservationPeopleCount', label: 'Nr. persone' },
    { id: 'reservationExpectedBudget', label: 'Budget (stimato)' },
    { id: 'reservationUserCodeValue', label: 'Codice PR' }];

  modalType: ModalType = ModalType.NEW_RESERVATION;

  eventFilter = 0;
  isMobileView = false;
  isTabletView = false;

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _user: UserService,
    private _generalService: GeneralService  ) {
  }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
    this.isTabletView = this._generalService.isTabletView();
    this.initData();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  public initData() {
    this.isLoading = true;
    var calls: Observable<any>[] = [
      this._api.getReservations(this.eventFilter,  0),//no filter
      this._api.getEvents()
    ];

    forkJoin(calls).pipe(
      catchError(err => { console.log(err); return err; }))
      .subscribe((data: any) => {
        this.reservations = data[0].reservations;        
        this.events = data[1].events;

        this.editBudget = false;
        this.isLoading = false;
      });
  }

  
  handleReservation(evt:any) {
    var button = evt.target;
    var status = 1;
    var reserveId = button.id.split("_")[1];
    var btnId = button.id;

    if (btnId.includes("Reservation")) {
      if (btnId.includes("btnAcceptReservation")) {
        status = 2;
        reserveId = reserveId;
      }
      if (btnId.includes("btnRefuseReservation")) {
        status = 3;
        reserveId = reserveId;
      }
      this._api.acceptReservation(reserveId, status).pipe(
        catchError(err => {
          this._modal.showErrorOrMessageModal(err.message);
          return err;
        }))
        .subscribe(data => {
          this.initData();
        });
    } else if (btnId.includes("Budget")) {
      if (btnId.includes("btnConfirmBudget")) {
        //api confirm expectedBudget
        var budget = this.reservations?.find(x => x.reservationId == reserveId)?.reservationExpectedBudget;

        if (budget != null)
          this.confirmBudget(reserveId, budget);

      } else if (btnId.includes("btnEditBudget")) {
        this.editBudget = true;
        
      } else if (btnId.toString().includes("btnOkEdtBudget")) {
        //api confirm edt budget

        var budget = this.reservations?.find(x => x.reservationId == reserveId)?.reservationRealBudget;
        
        if (budget != null)
          this.confirmBudget(reserveId, budget);
      }
    } 
  }

  confirmBudget(reserveId: number, budget: number) {

    this._api.confirmReservationBudget(reserveId, budget).pipe(
      catchError(err => { 
        return err;
      })).subscribe(data => {
        this.initData();
      });
  }

  addReservation() {
    this.getReservationData();
  }

  //Get data for useful for new reservation
  getReservationData() {
    const calls: Observable<any>[] = [
      this._api.getPrCustomers(),      
      this._api.getReservationTypes()
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.prCustomers = data[0];
        this.reservationTypes = data[1];
        this.initReservationInput();
        
      })
  }

  initReservationInput() {
    var modaViews: ModalViewGroup[] = [
      {
        type: ModalModelEnum.Dropdown, viewItems: [{
          viewId: "drpEvent", referenceId:"eventId", list: this.events, label: "Evento"
        }]
      },
      {
        type: ModalModelEnum.TextBox, viewItems: [
          { label: "Nome prenotazione", viewId: "txtReservationName", referenceId: "reservationName" },
          { label: "Nr. persone", viewId: "txtPeopleCount", referenceId: "peopleCount" },
          { label: "Budget (previsto)", viewId: "txtExpectedBudget", referenceId: "expectedBudget" },
          { label: "Note", viewId: "txtReservationNote", referenceId: "reservationNote" }
      ],
    },
    {
      type: ModalModelEnum.Dropdown, viewItems: [
        { label: "Tipo prenotazione", viewId: "drpreservationTypes", referenceId: "reservationTypes", list: this.reservationTypes }
      ]
    }];

    if (this._user.userIsInStaff()) {
      modaViews.push({
        type: ModalModelEnum.Table, viewItems: [{ label: "Prenota per", viewId: "tblPrCustomers", referenceId: "customerId", list: this.prCustomers }]
      })
    } else {
      modaViews.push({
        type: ModalModelEnum.TextBox, viewItems: [{ label: "Codice pr", viewId: "txtPrCode", referenceId: "userCode", defaultText: this._user.getCustomerPrCode() }]
      })
    }
    this._modal.showAddModal(this.onReservationAdded, "PRENOTAZIONE", modaViews);
  }

  onReservationAdded = (newReservation: any): void => {
    var reservation: ReservationPost = {
      eventPartyId: newReservation.get("eventId"),
      reservationExpectedBudget: newReservation.get("expectedBudget"),
      reservationPeopleCount: newReservation.get("peopleCount"),
      reservationName: newReservation.get("reservationName"),
      reservationUserCodeValue: newReservation.get("userCode"),
      reservationType: newReservation.get("reservationTypes")
    };

    if (newReservation.get("customerId") != null)
      //qui il discorso è leggermente diverso. Qui passo alla modal una lista di dati da selezionare quindi devo indicare che valore voglio
      //all'interno della lista. Siccome in questa casistica la prenotazione sarà effettuata sempre e SOLO per un utente, posso indicare manualmente
      //l'index 0, tanto ci sarà sempre solo una selezione
      reservation.reservationOwnerId = newReservation.get("customerId")![0];

    this._api.postReservation(reservation).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((message: any) => {
        //show success modal
        if (message != null)
          this._modal.showErrorOrMessageModal(message.message, "Prenotazione effettuata", true);

        this.initData();
      })
  }

  onFilterEventChange() {
    this._api.getReservations(this.eventFilter).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.reservations = data;
      });
  }
}
