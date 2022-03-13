import { ViewChild } from '@angular/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { AuthResponse, RegistrationRequest } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss', '../login/login.component.scss']
})
export class RegistrationComponent implements OnInit {

  @ViewChild("modalTemplate", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  registrationRequest: RegistrationRequest = { email: "", password: "", username: "", surname: "", name: "", prCode: "",phoneNumber: "+39", role:3, gender: ""};
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
    })    
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  } 

  register() {
    this.isLoading = true;
    this._api.register(this.registrationRequest).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal("Errore generale");
        return err;
      })).subscribe((data: AuthResponse) => {
        this.isLoading = false;
        if (data.operationSuccess) {
          this._modal.showErrorOrMessageModal("Registrazione avvenuta con successo.");

          setTimeout(function () {
            document.location.href = "http://localhost:4200/Login";
          }, 1500);

        } else {
          this._modal.showErrorOrMessageModal(data.message);
        }
          
      })
  }
}
