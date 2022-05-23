import { ViewContainerRef } from '@angular/core';
import { ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalImageBoxList, ModalModelEnum, ModalTextBoxList, ModalViewGroup } from '../../components/modal/modal.model';
import { EventParty, EventPartyView, NewTableOrderData, PrCustomerView, Reservation, ReservationPost, ReservationType } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.scss', '../../app.component.scss']
})
export class EventsComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?:ViewContainerRef;
  isLoading = false;

  events_full?: EventPartyView = {events:[]};
  events?: EventPartyView = {events:[]};
  eventFilterValue: string = "";

  txtListModal: ModalViewGroup = {type: ModalModelEnum.TextBox, viewItems:[]};
  imgBoxsModal: ModalViewGroup = { type: ModalModelEnum.ImageBox, viewItems: [] };

  addReservationModalViews: ModalViewGroup[] = [];
  addNewEventModalViews: ModalViewGroup[] = [];

  addModalTitle: string = "Nuovo evento";

  /****NEW RESERVATION MODAL INPUTS*****/
  reservationTxtList: ModalViewGroup = { type: ModalModelEnum.TextBox, viewItems: [] };
  reservationDrpList: ModalViewGroup = { type: ModalModelEnum.Dropdown, viewItems: [] };
  tablePrCustomer: ModalViewGroup = { type: ModalModelEnum.Table, viewItems: [] };
  reservationEvent = 0;
  /*********/

  showEventReservation = false;
  eventReservation?: Reservation;
  reservationTypes?: ReservationType[];

  constructor(private _api: ApiCaller,
    private _modalService: ModalService,
    private _router: Router,
    private _generalService: GeneralService  ) { }

  ngOnInit(): void {
    this.initData();    
  }

  ngAfterViewInit(): void {
    if (this.modalContainer != null) {
      this._api.setModalContainer(this.modalContainer);
    }    
  }

  initData() {
    this.isLoading = true;
    this._api.getEvents(true)
      .subscribe((data: any) => {
        this.events_full = data;
        
        this.events!.events = this.events_full?.events;

        this.isLoading = false;
      });    
  }
    
  openNewEventModal() {
    if (this.addNewEventModalViews != null && this.addNewEventModalViews.length > 0) {
      this.addNewEventModalViews = [];
    }

    this.addNewEventModalViews.push(
      {
        type: ModalModelEnum.TextBox, viewItems: [
          {label: "Nome *", viewId: "txtEventName", referenceId:"eventName"},
          {label: "Data *", viewId: "txtEventDate", referenceId:"eventDate"},
          {label: "Prezzo ingresso *", viewId: "txtEntrancePrice", referenceId:"eventEntrancePrice"},
          {label: "Prezzo tavolo *", viewId: "txTablePrice", referenceId:"eventTablePrice"}
        ]
      },
      {
        type: ModalModelEnum.Textarea, viewItems: [
          { label: "Descrizione evento *", viewId: "txtEventDescription", referenceId: "eventDescription" },
          { label: "Condizioni omaggio", viewId: "txtFreeEntrance", referenceId: "eventFreeEntrance" }
        ]
      },
      {
        type: ModalModelEnum.ImageBox, viewItems: [
          { viewId: "txtEventImg", referenceId: "eventImageCover", label: "Foto copertina *" },
          { viewId: "txtEventImg2", referenceId: "eventImagePreview", label: "Foto anteprima *" },
          { viewId: "txtEventImg3", referenceId: "eventImageDetailOne", label: "Foto dettaglio 1 *" },
          { viewId: "txtEventImg4", referenceId: "eventImageDetailTwo", label: "Foto dettaglio 2 *" },
          { viewId: "txtEventImg5", referenceId: "eventImageDetailThree", label: "Foto dettaglio 3 *" },
        ]
      }
      //{ type: ModalModelEnum.Table, viewItems: [{viewId: "tblCustomers", referenceId: "customers", label:"Prenota per", list: this.getPrCustomers()}]}
    )
    this._modalService.showAddModal<Map<string, any>>(this.onEventAdded, this.addModalTitle, this.addNewEventModalViews);
  }

  onEventAdded = (info: any): void => {
    this.isLoading = true;
    var eventDate = "";
    if (info.get("eventDate") != null)
      eventDate = this._generalService.rewriteDateToISO(info.get("eventDate"));
    else {
      this._modalService.showErrorOrMessageModal("Data non valida.", "ERRORE");
      return;
    }
      

    var newEvent: EventParty = {
      name: info.get("eventName"),
      description: info.get("eventDescription"),
      date: new Date(eventDate),
      entrancePrice: info.get("eventEntrancePrice"),
      tablePrice: info.get("eventTablePrice"),
      freeEntranceDescription: info.get("eventFreeEntrance")
    };

    if (info.get("eventImageCover") == null ||
      info.get("eventImagePreview") == null ||
      info.get("eventImageDetailOne") == null ||
      info.get("eventImageDetailTwo") == null ||
      info.get("eventImageDetailThree") == null) {

      this._modalService.showErrorOrMessageModal("Caricare le foto per l'evento.");
      this.isLoading = false;
      return;
    } else {
      newEvent.linkImage = [
        info.get("eventImageCover"),
        info.get("eventImagePreview"),
        info.get("eventImageDetailOne"),
        info.get("eventImageDetailTwo"),
        info.get("eventImageDetailThree")
      ]
    }
  
    this._api.postEvent(newEvent)
      .pipe(catchError(err => { this.isLoading = false;  return err;}))
     .subscribe(() => {
       this.isLoading = false;
       this._modalService.hideModal();
       this.initData();
     })
 }

  filterEventsForName() {
    if (this.eventFilterValue == "") {
      this.events!.events = this.events_full!.events;
      return;
    } 
    this.events!.events = this.events_full!.events!.filter(x => { return x.name?.toLowerCase().includes(this.eventFilterValue.toLowerCase()) });
  }

  openEventDetails(id: any) {
    this._router.navigateByUrl(`/Events/Details?eventId=${id}&editable=${this.events?.userCanAddEvent}`)
  }

  onReservationAdded = (info: any): void => {
    if (info != null) {
      var reservation: ReservationPost = {
        //Questi campi sono delle textbox quindi avrò sempre e solo un valore perchè non invio liste di dati.
        //Inoltre con un campo di tipo Map<any,any[]> se non viene gestita direttamente l'array per il cambio valore, ma viene usato
        //direttamente l'ngModel come in questo caso, viene fatta direttamente un'associazione chiave - valore
        reservationName: info.get("reservationName"),
        reservationExpectedBudget: info.get("reservationExpectedBudget"),
        reservationPeopleCount: info.get("reservationPeopleCount"),
        reservationType: info.get("reservationTypeId"),
        reservationUserCodeValue: info.get("reservationUserCodeValue"),
        eventPartyId: this.reservationEvent             
      }

      

      this._api.postReservation(reservation).pipe(
        catchError(err => {
          this._modalService.showErrorOrMessageModal(err.message);
          return err;
        })).subscribe((msgResponse:any) => {
          this.initData();
          this.reservationEvent = 0;
        })
    }
    
  }

  addReservation(eventId?: number) {
    this.initReservationViewData();
    this.reservationEvent = eventId!;
    //this.getReservationDataInsert(eventId!);
  }

  initReservationInputsViews(customers?:any[], reservationTypes?:any[]) {
    if (this.addReservationModalViews.length > 0) {
      this.addReservationModalViews = [];
    }

    this.addReservationModalViews.push(
      {
        type: ModalModelEnum.TextBox, viewItems: [
          {label:"Nome prenotazione" , referenceId:"reservationName"},
          { label: "Nr. persone", referenceId:"reservationPeopleCount"},
          { label: "Budget (stimato)", referenceId:"reservationExpectedBudget"},
          { label: "Codice PR", referenceId:"reservationUserCodeValue"},
        ]
      },
      {
        type: ModalModelEnum.Dropdown, viewItems: [
          { label: "Tipo prenotazione", viewId: "drpReservationType", referenceId: "reservationTypeId", list:reservationTypes}
        ]
      },
      {
        type: ModalModelEnum.Table, viewItems: [
          {viewId: "tblCustomers", referenceId:"reservationCustomer", list: customers, label: "Prenota per"}
        ]
      }
    )

    this._modalService.showAddModal(this.onReservationAdded, `Nuova prenotazione`, this.addReservationModalViews);
  }

  //Faccio comparire la modal solo quando ho caricato tutti i dati.
  //Questo perchè quando passo una views con una determinata lista allo showAddModal, i dati in essa rimangono fissi e non si aggiornano
  //Anche dopo che la chiamata è stata effettuata.
  //Ricordiamoci che la chiamata è asincrona quindi una volta avviata prosegue con il restante codice
  initReservationViewData() {
    const observables: Observable<any>[] = [
      this._api.getPrCustomers(),
      this._api.getReservationTypes()
    ]

    forkJoin(observables).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data:any) => {
        this.initReservationInputsViews(data[0], data[1])
      })
  }

  getPrCustomers(): PrCustomerView[] {
    var prCustomers: PrCustomerView[] = [];

    this._api.getPrCustomers().pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        prCustomers.push(data);    
      });
    
    return prCustomers;
  }

  editReservation(eventId: any) {
    if (!this.showEventReservation) {
      this._api.getReservationEvent(eventId).pipe(
        catchError(err => {
          this._modalService.showErrorOrMessageModal(err.message);
          return err;
        })).subscribe((data: any) => {
          this.eventReservation = data;
          this.showEventReservation = true;
        });
    } else {
      this.showEventReservation = false;
    }   
  }

  goToEvent(eventId: any) {
    this._router.navigateByUrl(`Events/Details?eventId=${eventId}`);
  }

  deleteEvent(eventId: any) {
    this.isLoading = true;

    this._api.deleteEvent(eventId).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((response: any) => {
        this.isLoading = false;
        if (response != null)
          this._modalService.showErrorOrMessageModal(response);
        else
          this.initData();

      })
  }
}
