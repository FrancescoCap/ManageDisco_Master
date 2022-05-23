import { ViewChild } from '@angular/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { LOCALSTORARE_LOGIN_HEADER, LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU, onLoginResponse, onMenuChange } from '../../app.module';
import {CookieConstants, LoginRequest } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss', '../../app.component.scss']
})
export class LoginComponent implements OnInit {

  @ViewChild("modalContainer", { static: false, read: ViewContainerRef }) modalContainer?: ViewContainerRef;


  loginRequest: LoginRequest = { email: "", password: "" };
  isMobileView = false;

  constructor(private _api: ApiCaller,
    private _generalService: GeneralService,
    private _cookie: CookieService,
    private _router: Router) {
  }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
   
    localStorage.setItem(LOCALSTORARE_LOGIN_HEADER, "Login");    
  }

  ngAfterViewInit() {
    this._cookie.deleteAll();
    onMenuChange.next(true);
    onLoginResponse.next("Login");
    this._api.setModalContainer(this.modalContainer!);    
  }

  public login() {
    this._api.login(this.loginRequest).pipe(
      catchError(err => {       
        return err;
      })).subscribe((data: any) => {               
        var headerString = "Bentornato " + data.body.userNameSurname + '<br>' + "Saldo punti: " + data.body.userPoints;
        onLoginResponse.next(headerString);
        onMenuChange.next(true);

        document.cookie = CookieConstants.ISAUTHENTICATED + "=true";
        localStorage.setItem(LOCALSTORARE_LOGIN_HEADER, headerString);
        localStorage.setItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU, "1");
        this._router.navigateByUrl("Home");
    });
  }

}
