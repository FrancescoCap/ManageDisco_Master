import { formatDate } from '@angular/common';
import { EventEmitter, ViewContainerRef } from '@angular/core';
import { Component, OnInit, ViewChild } from '@angular/core';
import { toArray } from 'rxjs';
import { concatMap } from 'rxjs';
import { of } from 'rxjs';
import { Observable } from 'rxjs/internal/Observable';
import { catchError, mergeMap} from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalModelList, ModalTextBoxList, ModalViewGroup } from '../../components/modal/modal.model';
import { TableViewDataModel } from '../../components/tableview/tableview.model';
import { GeneralMethods } from '../../helper/general';
import { EventParty, ModalType, PrCustomerView, Reservation, ReservationPost, ReservationType, Table } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-reservation',
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.scss', '../../app.component.scss']
})
export class ReservationComponent implements OnInit {

  tableData: TableViewDataModel = {
    headers: [
        { value: "Data" },
        { value: "Codice prenotazione" },
        { value: "Evento" },
        { value: "Codice PR" },
        { value: "Tipo prenotazione" },
        { value: "Nome prenotazione" },
        { value: "Nr. persone" },
        { value: "Budget previsto" },
        { value: "Stato" },
        { value: "Budget effettivo" },
        { value: "" }
      ],
    rows: [
      { }
    ]
  }

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
  tables?: Table[];
  reservationIdSelected = -1;

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
  tableRowPage = 10;

  calls?: Observable<any>;

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _user: UserService,
    private _generalService: GeneralService  ) {
  }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView() || this._generalService.isTabletView();
    this.initData();
    this.tableData.onDataListChange = new EventEmitter<any>();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  public getInitialCalls() {
    return of(this._api.getEvents(false), this._api.getReservations(this.eventFilter, 0));
  }

  public initData() {
    this.isLoading = true;
    this.calls = this.getInitialCalls();

    this.calls.pipe(   
      concatMap(data => {        
        return data;
      }),
      toArray(),
      catchError(err => { return err; }))
      .subscribe((data: any) => {
        this.events = data[0].events;
        this.reservations = data[1];
        this.setDataForTableView();

        this.editBudget = false;
        this.isLoading = false;
      });
  }

  setDataForTableView() {
    var editPermissions = this._user.userIsAdminstrator();
    
    if (this.tableData.rows.length > 0)
      this.tableData.rows = [];

    this.reservations?.forEach((x, y) => {
      this.tableData.rows.push({
        cells: [
          { value: formatDate(x.reservationDate!.toString(), "dd/MM/yyyy", "en-US") },
          { value: x.reservationCode! },
          { value: x.eventName! },
          { value: x.reservationUserCode! },
          { value: x.reservationTypeValue! },
          { value: x.reservationName! },
          { value: x.reservationPeopleCount! },
          { value: x.reservationExpectedBudget!, isCurrency: true },
          { value: x.reservationStatus! },
          { value: x.reservationRealBudget!, isCurrency: true },
          {
            value: "", icon: [
              { class: "fa fa-check", referenceId: `conf_${x.reservationId}`, isToShow: editPermissions && x.reservationStatusId != 2, onClickCallback: this.onConfirmReservation},  //confirm reservation
              {class: "fa fa-edit", referenceId: `edt_${x.reservationId}`, isToShow: editPermissions && x.reservationStatusId != 3, onClickCallback: this.onEditReservation},    //edit reservation (include budget edit) --> open modal
              { class: "fa fa-euro", referenceId: `bdg_${x.reservationId}`, isToShow: editPermissions && x.reservationStatusId != 3 && x.canAcceptBudget, onClickCallback: this.onConfirmBudgetInRow},    //direct confirm for budget
              { class: "fa fa-trash", referenceId: `del_${x.reservationId}`, isToShow: editPermissions && x.reservationStatusId != 3, onClickCallback: this.onRejectReservation}    //reject reservation
            ]
          }]
      })
    })
    this.tableData.onDataListChange?.emit(this.tableData);
  }

  onConfirmReservation = (reservationId: number): void => {    
    this.isLoading = true;
    this.calls = of(this._api.acceptReservation(reservationId, 2) /*hardcoded status equal 2 (for API is equal to confirmed)*/, this._api.getReservations(this.eventFilter, 0));

    this.calls.pipe(
      concatMap(value => { return value; }),
      toArray(),
      catchError(err => {
        this.isLoading = false;
        return err;
      }))
      .subscribe((data:any) => {
        this.reservations = data[1];
        //reload table
        this.setDataForTableView();
        this.isLoading = false;
      });
  }

  /* LOOKING FOR HAVE UNIQUE FUNCTION BETWEEN ONCONFIRMRESERVATION AND REJECT */
  onRejectReservation = (reservationId: number): void => {
    this.isLoading = true;
    this.calls = of(this._api.acceptReservation(reservationId, 3) /*hardcoded status equal 3 (for API is equal to reject)*/, this._api.getReservations(this.eventFilter, 0));

    this.calls.pipe(
      concatMap(value => { return value; }),
      toArray(),
      catchError(err => {
        this.isLoading = false;
        return err;
      }))
      .subscribe((data: any) => {
        this.reservations = data[1];
        //reload table
        this.setDataForTableView();
        this.isLoading = false;
      });
  }

  onConfirmBudgetInRow = (reservationId: number): void => {
    var budget = this.reservations?.find(x => x.reservationId == reservationId)?.reservationExpectedBudget;
    this._api.confirmReservationBudget(reservationId, budget!)
      .subscribe(data => {
        this.initData();
      });
  }

  onEditReservation = (reservationId: number): void => {
    var reservation = this.getReservationDetails(reservationId);

    if (this.isMobileView) {
      this.editBudget = true;
    } else {
      var views = GeneralMethods.getEditReservationModalViews(
        reservation.reservationName,
        reservation.reservationPeopleCount,
        reservation.reservationExpectedBudget,
        reservation.reservationRealBudget);

      this.reservationIdSelected = reservationId;

      this._modal.showAddModal(this.onReservationEditConfirm, "MODIFICA PRENOTAZIONE", views);
    }
  }

  getReservationDetails(reservationId:number):Reservation {
    if (reservationId == 0)
      return {};

    return this.reservations?.find(x => x.reservationId == reservationId)!;
  }

  onReservationEditConfirm = (data:any):void => {
    var reservation: ReservationPost = {
      reservationId: this.reservationIdSelected,
      reservationName: data.get("reservationName"),
      reservationPeopleCount: data.get("peopleCount"),
      reservationExpectedBudget: data.get("expectedBudget"),
      reservationRealBudget: data.get("realBudget")
    }
    this.editReservation(reservation);
  }

  onReservationMobileEditConfirm(reservationId:number, budget: number) {
    var reservationUpdated: ReservationPost = {
      reservationId: reservationId,
      reservationRealBudget: budget
    }

    this.editReservation(reservationUpdated);
  }

  editReservation(data: ReservationPost) {
    this.isLoading = true;
    this.calls = of(this._api.putReservation(data), this._api.getReservations(this.eventFilter, 0));

    this.calls.pipe(
      concatMap(value => {
        return value;
      }),
      toArray(),
      catchError(err => { this.isLoading = false; return err;})
    ).subscribe((data: any) => {
      this.reservations = data[1];
      this.setDataForTableView();
      this._modal.hideModal();
      this.editBudget = false;
      this.isLoading = false;
    })
  }

  handleReservation(evt: any) {
    console.log("Ciao")
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

    //gestione refreshToken su POST. Mi serve realmente? Nell'interceptor comunque è gestito l'errore 401 e qui, avendo solo una chiamata, verrebbe comunque fatta la post

    this._api.confirmReservationBudget(reserveId, budget)
      .subscribe(data => {
        this.initData();
      });
  }

  addReservation() {
    this.getReservationData();
  }

  //Get data for useful for new reservation
  getReservationData() {
    this.calls = of(this._api.getPrCustomers(), this._api.getReservationTypes(), this._api.getTables());

    this.calls.pipe(
      concatMap(values => { return values; }),
      toArray()
    ).subscribe((data: any) => {
        this.prCustomers = data[0];
        this.reservationTypes = data[1];
        this.tables = data[2];
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
        { label: "Tipo prenotazione", viewId: "drpreservationTypes", referenceId: "reservationTypes", list: this.reservationTypes },
        { label: "Posizione tavolo", viewId: "drpTables", referenceId: "tableId", list: this.tables }
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
    var reservation = this.getReservationObject(newReservation);

    if (newReservation.get("customerId") != null)
      //qui il discorso è leggermente diverso. Qui passo alla modal una lista di dati da selezionare quindi devo indicare che valore voglio
      //all'interno della lista. Siccome in questa casistica la prenotazione sarà effettuata sempre e SOLO per un utente, posso indicare manualmente
      //l'index 0, tanto ci sarà sempre solo una selezione
      reservation.reservationOwnerId = newReservation.get("customerId")![0];
    
    this.calls = of(this._api.postReservation(reservation), this._api.getReservations(this.eventFilter, 0));
    
    this.calls.pipe(
      concatMap(value => {  return value; }),
      toArray()
    ).subscribe((data: any) => {
      this._modal.hideModal();
        //show success modal
      if (data[0] != null)
        this._modal.showOperationResponseModal(data[0].message, "Prenotazione effettuata", false);
      //reload reservations
      this.reservations = data[1];
      this.setDataForTableView();
    })
  }

  onFilterEventChange() {    
    this._api.getReservations(this.eventFilter).pipe()
      .subscribe((data: any) => {
        this.reservations = data;
        this.setDataForTableView();
    });
  }

  getReservationObject(data:any):ReservationPost {
    return {
      eventPartyId: data.get("eventId"),
      reservationExpectedBudget: data.get("expectedBudget"),
      reservationPeopleCount: data.get("peopleCount"),
      reservationName: data.get("reservationName"),
      reservationUserCodeValue: data.get("userCode"),
      reservationType: data.get("reservationTypes"),
      tableId: data.get("tableId")
    };
  }
}
