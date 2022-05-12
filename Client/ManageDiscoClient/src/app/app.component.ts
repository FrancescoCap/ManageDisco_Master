import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { concatMap, mergeMap } from 'rxjs';
import { toArray } from 'rxjs';
import { forkJoin, Observable, of, Subject } from 'rxjs';
import { ApiCaller } from './api/api';
import { client_URL, LOCALSTORARE_LOGIN_HEADER, LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU, onLoginResponse } from './app.module';
import { HeaderMenu, HomeInfo } from './model/models';
import { GeneralService } from './service/general.service';
import { UserService } from './service/user.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'ManageDiscoClient';

  menu?: HeaderMenu[];
  homeInfo?: HomeInfo;
  isMobileView = false;
  isAuthenticated = false;
  isMenuLoaded = false;

  instagramContactTypeId?: number;
  facebookContactTypeId?: number;
  authorizationStateString = "Login";
  isLoginHeaderMenuEnabled = false;

  constructor(private _api: ApiCaller,    
    private _generalService: GeneralService,
    private _user: UserService) {}

  ngOnInit(): void {
    //this.redirectToHomeifLogged();
    this.isMobileView = this._generalService.isMobileView();
    this.isAuthenticated = this._user!.userIsAuthenticated();
    this.getFooterAndHeader();
    
    this.authorizationStateString = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER)!;
    onLoginResponse.next(this.authorizationStateString);
    this.isLoginHeaderMenuEnabled = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU) == "1";
  }

  ngAfterViewInit() {
    onLoginResponse.subscribe((value: string) => {
      this.authorizationStateString = value;
    })
  }

  getFooterAndHeader() {
    var calls: Observable<any> = of(
      this._api.getHomeInfo(),
      this._api.getMenu()
    )

    calls.pipe(
      concatMap(value => { return value; }),
      toArray()
    )
      .subscribe((data: any) => {
      this.homeInfo = data[0]
      this.menu = data[1] as HeaderMenu[];
      this.instagramContactTypeId = this.homeInfo!.contacts?.find(x => x.contactTypeDescription?.includes("Instagram"))?.contactTypeId;
      this.facebookContactTypeId = this.homeInfo!.contacts?.find(x => x.contactTypeDescription?.includes("Facebook"))?.contactTypeId;     
    });

  }

  onLoginLogoutClick() {
    console.log("Click")
    if (this._user!.userIsAuthenticated())
      this.logout();
    else
      this.goToLoginPage();
  }

  goToLoginPage() {
    //uso il document.location altrimenti quando vado nella maschera di login non si aggiorna il menù
    document.location.href = `${client_URL}/Login`
  }

  logout() {
    this._api.logout().subscribe(() => {
      localStorage.removeItem(LOCALSTORARE_LOGIN_HEADER);
      localStorage.removeItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU);
      
      this.goToLoginPage();
    })
  }

}
