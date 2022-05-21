import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { concatMap, mergeMap } from 'rxjs';
import { toArray } from 'rxjs';
import { forkJoin, Observable, of, Subject } from 'rxjs';
import { ApiCaller } from './api/api';
import { client_URL, LOCALSTORARE_LOGIN_HEADER, LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU, onLoginResponse, onMenuChange } from './app.module';
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
  isTabletView = false;
  isAuthenticated = false;
  isMenuLoaded = false;

  instagramContactTypeId?: number;
  facebookContactTypeId?: number;
  authorizationStateString = "Login";
  isLoginHeaderMenuEnabled = false;

  showChildLogout = false;

  constructor(private _api: ApiCaller,    
    private _generalService: GeneralService,
    private _user: UserService,
    private _router:Router  ) { }

  ngOnInit(): void {
    //this.redirectToHomeifLogged();
    this.isMobileView = this._generalService.isMobileView() || this._generalService.isTabletView();
    this.isAuthenticated = this._user!.userIsAuthenticated();
    this.getFooterAndHeader();
    
    this.authorizationStateString = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER)!;
    onLoginResponse.next(this.authorizationStateString);
    this.isLoginHeaderMenuEnabled = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU) == "1";
  }

  ngAfterViewInit() {
    this.startListeners();
  }

  startListeners() {
    onLoginResponse.subscribe((value: string) => {
      this.authorizationStateString = value;
    });

    onMenuChange.subscribe(() => {
      this._api.getMenu().subscribe((menu: HeaderMenu[]) => {
        this.menu = menu;
      })
    });
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
    if (this._user!.userIsAuthenticated())
      this.logout();
    else
      this.goToLoginPage();
  }

  goToLoginPage() {
    onMenuChange.next(true);
    this._router.navigateByUrl("Login");
  }

  logout() {
    this._api.logout().subscribe(() => {
      localStorage.removeItem(LOCALSTORARE_LOGIN_HEADER);
      localStorage.removeItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU);
      
      this.goToLoginPage();
    })
  }

  openMobileChildLogout() {
    this.showChildLogout = !this.showChildLogout;
  }

}
