import { Component, OnInit, ViewChild, ViewContainerRef, ViewEncapsulation } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../../api/api';
import { EventParty, LoginRequest, ModalType, PrCustomerView, ReservationPost, ReservationType } from '../../../model/models';
import { ModalService } from '../../../service/modal.service';
import SwiperCore, { EffectFade, Autoplay, Pagination, Navigation, Scrollbar } from "swiper";
import { ModalModelEnum, ModalViewGroup } from '../../../components/modal/modal.model';
import { Observable } from 'rxjs';
import { forkJoin } from 'rxjs';
import { UserService } from '../../../service/user.service';
import { GeneralMethods } from '../../../helper/general';

SwiperCore.use([EffectFade, Autoplay, Pagination, Navigation, Scrollbar]);

@Component({
  selector: 'app-event-detail',
  templateUrl: './event-detail.component.html',
  styleUrls: ['./event-detail.component.scss', '../../../app.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EventDetailComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  isLoading = false;

  modaViews: ModalViewGroup[] = [];
  reservationType?: ReservationType[];
  prCustomers?: PrCustomerView[];

  event?: EventParty;
  eventId: any = 0;
  editPriceMode = false;
  editPriceModel = { entrancePrice: 0, tablePrice: 0, freeEntranceDescription: "", eventDescription: "" };
  editDescription = false;
  areDetailsEditableFromUser: boolean = false;
  userIsAdministrator: boolean = false;

  //[ngStyle] = "{'visibility': areDetailsEditableFromUser == true ? 'visible':'hidden'}"
  constructor(private _api: ApiCaller,
    private route: RouterOutlet,
    private modal: ModalService,
    private router: Router,
    private user: UserService ) { }

  ngOnInit(): void {
    this.route.activatedRoute.queryParams.subscribe(params => {
      this.eventId = params["eventId"];     
      this.areDetailsEditableFromUser = this.user.userIsAdminstrator();
    })
    this.initData();
  }

  ngAfterViewInit() {
    this.modal.setContainer(this.modalContainer!);
  }

  initData() {
    this.isLoading = true;
    this._api.getEventDetail(this.eventId)
    .subscribe((data:any)  => {
      this.event = data;
      this.editPriceMode = false;
      this.editDescription = false;
      this.userIsAdministrator = this.user.userIsAdminstrator();
      this.isLoading = false;
    });
  }

  //Abiilita la visualizzazione di modifica prezzi
  editPrice() {
    this.editPriceMode = !this.editPriceMode;
  }

  editEventDescription() {
    this.editDescription = !this.editDescription;
  }

  editPriceConfirm() {

    this._api.editEventPrices(this.eventId, this.event).pipe(
      catchError(err => {
        this.modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe(() => {
        this.initData();
      });
  }

  goTo(target: any) {
    if (target.id === "events")
      this.router.navigateByUrl("/Events");
    else if (target.id == "delete") {
      this._api.putCancelEvent(this.eventId).pipe(
        catchError(err => {
          this.modal.showErrorOrMessageModal(err.message);
          return err;
        })).subscribe(() => {
          this.router.navigateByUrl("/Events");
        })
    }
  }

  addReservation() {    
    this.getReservationData();   
  }

  //Get data for useful for new reservation
  getReservationData() {
    const calls: Observable<any>[] = [
      this._api.getReservationTypes(),
      this._api.getPrCustomers()
    ]
    
    if (!this.user.userIsAuthenticated()) {
      this.initLoginModal();
    } else {
      forkJoin(calls).pipe(
        catchError(err => {
          this.modal.showErrorOrMessageModal(err.message);
          return err;
        })).subscribe((data: any) => {
          this.reservationType = data[0];
          this.prCustomers = data[1];

          this.initReservationInput();
          this.modal.showAddModal(this.onReservationAdded, "PRENOTAZIONE", this.modaViews);
        })
    }
    
  }

  initLoginModal() {
    this.modal.showAddModal(this.onLogin, "LOGIN", GeneralMethods.getLoginModalViews());
  }

  onLogin = (info: any): void => {
    var loginReq: LoginRequest = {
      email: info.get("email"),
      password: info.get("password")
    }

    this._api.login(loginReq).pipe(
      catchError(err => {
        this.isLoading = false;        
        return err;
      })).subscribe((data: any) => {
        this.modal.hideModal();
        this.initData();
      })
  }

  initReservationInput() {
    this.modaViews = [{
      type: ModalModelEnum.TextBox, viewItems: [
        { label: "Nome prenotazione", viewId: "txtReservationName", referenceId: "reservationName" },
        { label: "Nr. persone", viewId: "txtPeopleCount", referenceId: "peopleCount" },
        { label: "Budget (previsto)", viewId: "txtExpectedBudget", referenceId: "expectedBudget" },
        { label: "Note", viewId: "txtReservationNote", referenceId: "reservationNote" }
      ],
    },
    {
      type: ModalModelEnum.Dropdown, viewItems: [
        {label: "Tipo prenotazione", viewId:"drpReservationType", referenceId:"reservationType", list:this.reservationType}
      ]
      }];

    if (this.user.userIsInStaff()) {
     
      this.modaViews.push({
        type: ModalModelEnum.Table, selector:"tblCustomer", multiSelect: false, viewItems: [{ label: "Prenota per", viewId: "tblPrCustomers", referenceId: "customerId", list: this.prCustomers }]
      });
      this.modaViews.push({
        type: ModalModelEnum.Checkbox, selector: "checkBoxForMe", viewItems: [{label: "Prenota per me", viewId:"chbForMe", referenceId:"chbIsForMe"}]
      })
    } else {      
      this.modaViews.push({
        type: ModalModelEnum.TextBox, viewItems: [{ label: "Codice pr", viewId: "txtPrCode", referenceId: "userCode", defaultText: this.user.getCustomerPrCode() }]
      });
    }
  }

  onReservationAdded = (newReservation:any): void => {
    var reservation: ReservationPost = {
      eventPartyId: this.eventId,
      reservationExpectedBudget: newReservation.get("expectedBudget"),
      reservationPeopleCount: newReservation.get("peopleCount"),     
      reservationName: newReservation.get("reservationName"),
      reservationUserCodeValue: newReservation.get("userCode"),
      reservationType: newReservation.get("reservationType")
    };

    if (newReservation.get("customerId") != null)
      //qui il discorso ?? leggermente diverso. Qui passo alla modal una lista di dati da selezionare quindi devo indicare che valore voglio
      //all'interno della lista. Siccome in questa casistica la prenotazione sar?? effettuata sempre e SOLO per un utente, posso indicare manualmente
      //l'index 0, tanto ci sar?? sempre solo una selezione
      reservation.reservationOwnerId = newReservation.get("customerId")![0];

    this._api.postReservation(reservation).pipe(
      catchError(err => {       
        return err;
      })).subscribe((message: any) => {     
        //show success modal
        if (message != null) {
          this.modal.hideModal();
          this.modal.showOperationResponseModal(message.message, "PRENOTAZIONE");          
        }
          
      })
  }

  onFreeEntraceRequest() {
    //Non apro una modal perch?? per richiedere l'omaggio necessito di essere registrato, quindi ho gi?? tutte le info che mi servono
    this.isLoading = true;
    this._api.getFreeEntrance(this.eventId).pipe(
      catchError(err => {
        this.isLoading = false;
        //Gestione messaggio d'errore su codice http 400
       
        return err;
      })).subscribe((data: any) => {
        this.isLoading = false;        
        this.onFreeEntranceSubscription();       
      })
  }

  onFreeEntranceSubscription() {
    const formData = new FormData();
    //get whatsapp message
    this._api.getSubscription(formData).pipe(
      catchError(err => {
        this.modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data:any) => {
        this.modal.showErrorOrMessageModal("La richiesta ?? avvenuta con successo. Controlla il tuo cellulare per il link al QR code da esibire all'ingresso", "OMAGGIO");
      })
  }

  startEditDesctiption() {
    if (this.user.userIsAdminstrator())
      this.editDescription = true;
  }

  closeFreeEntrance() {
    this._api.closeFreeEntrance(this.eventId).subscribe((data: any) => {
      this.initData();
    })
  }
}
