import { Component, OnInit } from '@angular/core';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelList, ModalTextBoxList } from '../../components/modal/modal.model';
import { EventParty, ModalType, Reservation, ReservationPost, ReservationType } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-reservation',
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.scss', '../../app.component.scss']
})
export class ReservationComponent implements OnInit {

  openNewResModal = false;
  newResModaltitle = "Nuova prenotazione";
  editBudget: boolean = false;
 
  reservations?: Reservation[];
  events?: EventParty[];
  reservationTypes?: ReservationType[];
  newReservation: ReservationPost = {eventPartyId:0, reservationPeopleCount: 0, reservationExpectedBudget: 0, reservationUserCodeValue: ""};

  modalModelLists: ModalModelList[] = [];
  modalTextBoxLists: ModalTextBoxList[] = [
    { id: 'reservationName', label: 'Nome prenotazione' },
    { id: 'reservationPeopleCount', label: 'Nr. persone' },
    { id: 'reservationExpectedBudget', label: 'Budget (stimato)' },
    { id: 'reservationUserCodeValue', label: 'Codice PR' }];

  modalType: ModalType = ModalType.NEW_RESERVATION;

  eventFilter = 0;

  constructor(private api: ApiCaller,
        private _modalService:ModalService) {
  }

  ngOnInit(): void {
    this.initData();
  }

  public initData() {

    var calls: Observable<any>[] = [
      this.api.getReservations(this.eventFilter,0),//no filter
      this.api.getEvents(),
      this.api.getReservationTypes()
    ];

    forkJoin(calls).pipe(
      catchError(err => { console.log(err); return err; }))
      .subscribe((data: any) => {
        this.reservations = data[0];        
        this.events = data[1].events;
        this.reservationTypes = data[2];
        this.editBudget = false;
        this.initModalDropDown();
      });
  }

  addReservation(event: Map<string,string>) {
    this.newReservation.reservationName = event.get("reservationName");
    this.newReservation.reservationPeopleCount = Number.parseInt(event.get("reservationPeopleCount") || "1");
    this.newReservation.reservationExpectedBudget = Number.parseFloat(event.get("reservationExpectedBudget") || "0");;
    this.newReservation.reservationUserCodeValue = event.get("reservationUserCodeValue");
    this.newReservation.reservationType = Number.parseInt(event.get("reservationTypeId") || "1");
    this.newReservation.eventPartyId = Number.parseInt(event.get("eventId") || "0");

    this.modalType = ModalType.NEW_RESERVATION;
    this.api.postReservation(this.newReservation).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(data => {
        this.openNewResModal = false;        
        this.initData();
      })
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
      this.api.acceptReservation(reserveId, status).pipe(
        catchError(err => {
          console.log(err);
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

    this.api.confirmReservationBudget(reserveId, budget).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(data => {
        this.initData();
      });
  }

  openNewReservationModal() {
    this.initModalDropDown();
    //this._modalService.showAddModal()
  }

  initModalDropDown() {
    //Rinizializzo ogni volta la lista a vuota perchè altrimenti le dropdown nuove si aggiungono a quelle vecchie
    this.modalModelLists = [];
    var modalListEvents = new ModalModelList();
    modalListEvents.dropId = "drpEvent";
    modalListEvents.label = "Evento";
    modalListEvents.id = "eventId";
    modalListEvents.valueDisplay = "name";
    modalListEvents.list?.push(this.events);

    var modalListResTypes = new ModalModelList();
    modalListResTypes.dropId = "drpResType";
    modalListResTypes.label = "Tipo prenotazione";
    modalListResTypes.id = "reservationTypeId";
    modalListResTypes.valueDisplay = "reservationTypeString";
    modalListResTypes.list?.push(this.reservationTypes);

    this.modalModelLists?.push(modalListEvents);
    this.modalModelLists?.push(modalListResTypes);
  }

  onModalClose(event:any) {
    this.openNewResModal = event;
  }

  dropEventChange() {
    this.initData();
  }
}
