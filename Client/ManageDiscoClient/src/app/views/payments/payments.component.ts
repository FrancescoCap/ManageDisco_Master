import { ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError, concatMap, Observable, of, toArray } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { ModalType, PaymentOverview, PaymentPost, ReservationPayments, User } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-payments',
  templateUrl: './payments.component.html',
  styleUrls: ['./payments.component.scss', '../../app.component.scss']
})
export class PaymentsComponent implements OnInit {

  isLoading = false;

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  showDetails: boolean[] = [];
  userIdExpanded = "";

  payments?: PaymentOverview[];
  paymentsDetails?: ReservationPayments[];
  collaborators?: User[];

  paymentInfo: PaymentPost = { reservationPaymentAmount: 0, userId: '' };
  modalType: ModalType = ModalType.NEW_PAYMENT;
  addNewPayment = false;
  newPaymentTitle = "NUOVO PAGAMENTO";

  newPaymentAmount = 0;
  userCanPay = false;
  isMobileView = false;
  isTabletView = false;

  paymentDetailsMap: Map<any, boolean> = new Map<any, boolean>();
  selectedPayment?: any;
  modalViews?: ModalViewGroup[];
  userIsAdministrator = false;
  calls?: Observable<any>;

  constructor(private _api: ApiCaller,
    private _services: GeneralService,
    private _modal: ModalService,
    private _user:UserService  ) { }

  ngOnInit(): void {
    this.isMobileView = this._services.isMobileView();
    this.isTabletView = this._services.isTabletView();
    this.userIsAdministrator = this._user.userIsAdminstrator();

    if (this.userIsAdministrator) {
      if(this.isMobileView || this.isTabletView)
        this.getCollaborators();
      else
        this.initData();

    } else if (this._user.userIsInStaff()) {
      this.getPaymentsOverviewDetails();      
        this.initData();
    }
    
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initData() {
    this.isLoading = true;
    this.initDetailsFlags();
    this.getPaymentOverview();
  }

  expandRow(event: any, idx:number) {
    this.showDetails[idx] = !this.showDetails[idx];
    if (this.showDetails[idx]) {
      this.userIdExpanded = this._services.getIdFromView(event.target.id);

      this.getPaymentsOverviewDetails();
    } else {
      this.userIdExpanded = "";
      this.showDetails[idx] = false;
    }
  }

  addPayment() { //event = newPaymentAmount
    this.modalViews = [
      {
        type: ModalModelEnum.TextBox, viewItems: [
          { viewId: "txtPaymentAmount", label: "Importo", referenceId: "paymentAmount" },
          { viewId: "txtPaymentDescription", label: "Descrizione", referenceId: "paymentDescription" }
        ]
      }
    ]
    
    this._modal.showAddModal(this.onPaymentAdded, "Nuovo pagamento", this.modalViews, ModalType.NEW_PRODUCT);  
  }

  onPaymentAdded = (info: any): void => {
    this.isLoading = true;
    this.paymentInfo = this.getPaymentObject(info);

    this.calls = of(this._api.postPayment(this.paymentInfo),
      this._api.getPaymentsOverview(""),
      this._api.getPaymentsOverviewDetails(this.userIdExpanded));

    this.calls
      .pipe(
        concatMap(value => { return value }),
        toArray(),
        catchError(err => {
          this.isLoading = false; return err;
        }))
      .subscribe((data:any) => {
        this.payments = data[1];
        this.paymentsDetails = data[2];
        this.initDetailsFlags();

        this.addNewPayment = false;
        this._modal.hideModal();
        this.isLoading = false;
        //this.loadPaymentsDetail(this.userIdExpanded);        
      });
  }

  private getPaymentObject(data: any) {
    return {
      reservationPaymentAmount: data.get("paymentAmount"),
      reservationPaymentDescription: data.get("paymentDescription"),
      userId: this.userIdExpanded
    };
  }

  //reset user selection
  initDetailsFlags() {
   this.payments?.forEach(x => {
      this.paymentDetailsMap.set(x.userId, false);
    })
  }

  openModalNewPayment(userId:any) {
    this.addNewPayment = true;
    this.paymentInfo.userId = userId;
  }

  onModalClose(event:any) {
    this.addNewPayment = event;
  }

  loadPaymentsDetail(userId: any) {
    
    if (!this._services.isMobileView() && !this._services.isTabletView()) {
      this.userIdExpanded = userId;
      this.paymentDetailsMap.clear();
      this.paymentDetailsMap.set(this.userIdExpanded, true);
    } else {
      //Value retrieved from dropdown change event
      this.userIdExpanded = userId.target.value;
      this.getPaymentOverview();
    }

    
    this.getPaymentsOverviewDetails();
  }

  getCollaborators() {
    this.isLoading = true;
    this._api.getCollaborators()
      .subscribe((data:any[]) => {
        this.collaborators = data;

        this.isLoading = false;
      })
  }
   
  getPaymentOverview() {
    this._api.getPaymentsOverview(this.userIdExpanded)
      .subscribe((data: any) => {
        this.payments = data;
        this.initDetailsFlags();

        this.userCanPay = this._user.userIsAdminstrator();
        this.isLoading = false;
      })
  }
  
  getPaymentsOverviewDetails() {   
    this._api.getPaymentsOverviewDetails(this.userIdExpanded)
      .subscribe(data => {
        this.paymentsDetails = data;
      });
  }
}



