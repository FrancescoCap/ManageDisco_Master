import { Route } from '@angular/compiler/src/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterModule, RouterOutlet } from '@angular/router';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { GeneralMethods } from '../../helper/general';
import { LoginRequest, ModalType } from '../../model/models';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-coupon',
  templateUrl: './coupon.component.html',
  styleUrls: ['./coupon.component.scss']
})
export class CouponComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  userId?: any;
  action?: any;
  base64Coupon?: string
  isLoading = false;  

  constructor(private _router: RouterOutlet,
    private _api: ApiCaller,
    private _modal: ModalService,
    private _user: UserService) { }

  ngOnInit(): void {
    this.isLoading = true;
   
    this._router.activatedRoute.queryParams.subscribe(params => {
      this.userId = params["refer"];     
      this.action = params["action"];      
    })
  }

  ngAfterViewInit() {
    this._modal.setContainer(this.modalContainer!);

    if (this._user.userIsAuthenticated()) {
      if (this.action != null && this.action == "validate" && this._user.userIsInStaff())
        this.getCouponInfo();
      else if (this.action != null && this.action == "phoneConfirm")
        this.confirmPhone();
      else
        this.getCoupon();
    }
    else {
      this.isLoading = false;
      this._modal.showAddModal(this.onLogin, "LOGIN", GeneralMethods.getLoginModalViews(), ModalType.LOGIN);
    }
    
  }

  onLogin = (response: any): void => {
    const loginInfo: LoginRequest = {
      email: response.get("email"),
      password: response.get("password")
    }
    this.isLoading = true;
    this._api.login(loginInfo).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((response: any) => {
        if (this.action != null && this.action == "phoneConfirm") {
          this.confirmPhone();
        }else
          this.getCouponInfo();
      })
  }

  //Questo serve solo per chi controlla i coupon
  initData() {
    this._api.getUserInfoFromId(this.userId).pipe(
      catchError(err => {
        this._modal.showOperationResponseModal(err.message, "ERRORE", true);
        return err;
      })).subscribe(message => {
        this._modal.showOperationResponseModal("Coupon valido", "VERIFICA", false);
      })
  }

  //Questo serve solo per l'utente-cliente che deve recuperare il coupon da presentare
  getCoupon() {
    this._api.getCoupon(this.userId).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.base64Coupon = GeneralMethods.normalizeBase64(data.value);
        this.isLoading = false;
      })
  }

  getCouponInfo() {
    this.isLoading = true;

    this._api.getCouponInfo(this.userId).pipe(
      catchError(err => {
        this.isLoading = false;
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.isLoading = false;
        if (data.message != null)
          console.log(data.message)
        this.showValidCouponModal(data);

      })
  }

  showValidCouponModal(user:any) {
    var views: ModalViewGroup[] = [
      {
        type: ModalModelEnum.Label, viewItems: [
          { label: "Nome", defaultText: user.name },
          { label: "Cognome", defaultText: user.surname },
          { label: "Data di nascita", defaultText: "" }
        ]
      }
    ];

    this._modal.showOperationResponseModal("", "Coupon confermato!", false, views);
  }

  confirmPhone() {
    this.isLoading = true;
    this._api.confirmUserPhoneNumber(this.userId).pipe(
      catchError(err => {
        return err;
      })).subscribe((data: any) => {
        this.isLoading = false;
        var views: ModalViewGroup[] = [];

        this._modal.showOperationResponseModal(data.message, "ESITO", !data.operationSuccess, views);
      })
  }

}
