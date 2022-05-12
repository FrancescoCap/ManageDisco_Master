import { ViewChild } from '@angular/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { client_URL } from '../../app.module';
import { LoginResponse, RegistrationRequest } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss', '../login/login.component.scss']
})
export class RegistrationComponent implements OnInit {

  @ViewChild("modalTemplate", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  registrationRequest: RegistrationRequest = { email: "", password: "", username: "", surname: "", name: "", prCode: "",phoneNumber: "", gender: ""};
  isCustomerRegistration = true;
  isLoading = false;
  repeatedPassword = "";

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _route: ActivatedRoute  ) { }

  ngOnInit(): void {
    this._route.queryParams.subscribe(params => {
      if (params["type"] != null)
        this.isCustomerRegistration = true;
      if (params["code"] != null)
        this.registrationRequest.prCode = params["code"];
    })    
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  } 

  register() {
    this.isLoading = true;
    this._api.register(this.registrationRequest).pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: LoginResponse) => {
        this.isLoading = false;
        if (data.operationSuccess) {
          this._modal.showOperationResponseModal("Registrazione avvenuta con successo.", "Registrazione", false, undefined, this.onRegistrationResponseModalClose);         

        } else {
          this._modal.showErrorOrMessageModal(data.message);
        }
          
      })
  }

  onRegistrationResponseModalClose = (data: any): void => {
    setTimeout(function () {
      document.location.href = client_URL + "/Login";
    }, 1000);
  }
}
