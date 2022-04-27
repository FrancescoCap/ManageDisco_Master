import { ViewChild } from '@angular/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Route, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { client_URL } from '../../app.module';
import { AuthResponse, LoginRequest } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  @ViewChild("modalContainer", { static: false, read: ViewContainerRef }) modalContainer?: ViewContainerRef;

  loginRequest: LoginRequest = { email: "", password: "" };
  isMobileView = false;

  constructor(private _api: ApiCaller,
    private _router: Router,
    private _modal: ModalService,
    private _generalService: GeneralService,
    private _user: UserService  ) { }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  public login() {
    this._api.login(this.loginRequest).pipe(
      catchError(err => {       
        return err;
      })).subscribe((data: AuthResponse) => {       
        document.location.href = `${client_URL}/Home`;
        
    });
  }

}
