import { ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ApiHttpService } from '../../api/http';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { ModalType, PaymentOverview, PaymentPost, ReservationPayments } from '../../model/models';
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

  paymentInfo: PaymentPost = { reservationPaymentAmount: 0, userId: '' };
  modalType: ModalType = ModalType.NEW_PAYMENT;
  addNewPayment = false;
  newPaymentTitle = "NUOVO PAGAMENTO";

  newPaymentAmount = 0;
  userCanPay = false;

  paymentDetailsMap: Map<any, boolean> = new Map<any, boolean>();
  selectedPayment?: any;
  modalViews?: ModalViewGroup[];

  constructor(private _api: ApiCaller,
    private _services: GeneralService,
    private _modal: ModalService,
    private _user:UserService  ) { }

  ngOnInit(): void {
    this.initData();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initData() {
    this.isLoading = true;

    this._api.getPaymentsOverview().pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data:any) => {
        this.payments = data;
        this.initDetailsFlags();

        this.userCanPay = this._user.userIsAdminstrator();

        setTimeout(() => {
          this.isLoading = false;
        }, 1000)
       
      })
  }

  expandRow(event: any, idx:number) {
    this.showDetails[idx] = !this.showDetails[idx];
    if (this.showDetails[idx]) {
      this.userIdExpanded = this._services.getIdFromView(event.target.id);

      this._api.getPaymentsOverviewDetails(this.userIdExpanded).pipe(
        catchError(err => {
          console.log(err);
          return err;
        })).subscribe(data => {
          this.paymentsDetails = data;
        });
    } else {
      this.userIdExpanded = "";
      this.showDetails[idx] = false;
    }
  }

  addPayment() { //event = newPaymentAmount
    this.modalViews = [
      { type: ModalModelEnum.TextBox, viewItems: [{viewId:"txtPaymentAmount", label:"Importo", referenceId:"paymentAmount"}] }
    ]
    
    this._modal.showAddModal(this.onPaymentAdded, "Nuovo pagamento", this.modalViews, ModalType.NEW_PRODUCT);
    //this.paymentInfo.reservationPaymentAmount = event;
    //this._api.postPayment(this.paymentInfo).pipe(
    //  catchError(err => {
    //    console.log(err);
    //    return err;
    //  })).subscribe(data => {
    //    this.addNewPayment = false;
    //    this.initData();
        
    //  });
  }

  onPaymentAdded = (info: any): void => {
    this.paymentInfo = {
      reservationPaymentAmount: info.get("paymentAmount"),
      userId: this.userIdExpanded
    }

    this._api.postPayment(this.paymentInfo).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(data => {
        this.addNewPayment = false;
        this.initData();
      });
  }

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
    this.userIdExpanded = userId;
    this.initDetailsFlags();

    this._api.getPaymentsOverviewDetails(userId).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(data => {
        this.paymentsDetails = data;

        this.paymentDetailsMap.set(userId, true);
      });
  }

}


