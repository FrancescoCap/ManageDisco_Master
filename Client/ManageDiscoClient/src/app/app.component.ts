import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { ApiCaller } from './api/api';
import { LOCALSTORARE_LOGIN_HEADER, LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU, onLoginResponse, onMenuChange } from './app.module';
import { CookieConstants, HeaderMenu, HomeInfo } from './model/models';
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
    private _router: Router,
    private _cookieService:CookieService) { }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView() || this._generalService.isTabletView();
    this.getFooter();
    
    this.authorizationStateString = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER)!;
    onLoginResponse.next(this.authorizationStateString);
    this.isLoginHeaderMenuEnabled = localStorage.getItem(LOCALSTORARE_LOGIN_HEADER_ENABLE_MENU) == "1";
    
  }

  ngAfterViewInit() {
    this.startListeners();
    onMenuChange.next(false);    
  }

  startListeners() {
    onLoginResponse.subscribe((value: string) => {
      this.authorizationStateString = value;
    });

    onMenuChange.subscribe((startCall: boolean) => {
      
      if (startCall) {
        this._cookieService.delete(CookieConstants.MENU_STATE);
        this._api.getMenu().subscribe((menu: HeaderMenu[]) => {
          this.menu = menu;
          //save menu when is not necessary refresh tabs
          var menuString = this.serializeMenuInCookie();
          var menuCookie = `${menuString}`;
          this._cookieService.set(CookieConstants.MENU_STATE,menuCookie);
        });
      } else {
        this.deserializeMenuCookie();
      }
    });
  }

  getFooter() {
    this._api.getHomeInfo().subscribe((data: any) => {
      this.homeInfo = data;
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

  serializeMenuInCookie(): string {
    var menuString: string = "";

    this.menu?.forEach((h, hi) => { //hi = headerIndex
      menuString += `${h.header}|${h.link}|${h.icon}|${h.child != null ? h.child?.length : 0}`;

      if (h.child != null && h.child?.length > 0) {
        h.child.forEach(c => {
          menuString += `:${c.title}|${c.link}|${c.icon}`;
        });
      }
      //I don't know why menu length starts from 1 and not 0
      //new menu tab
      menuString += hi < this.menu!.length-1 ? "," : "";

    });
    return menuString;
  }

  deserializeMenuCookie() {
    var deserializedMenu: HeaderMenu[] = [];

    var menuString = this._cookieService.get(CookieConstants.MENU_STATE);
   
    //get tabs
    var headers = menuString.split(",");
   
    headers.forEach(h => {
      var menuItem: HeaderMenu = {};
      var headerValues = h.split("|");
     
      menuItem.header = headerValues[0];
      menuItem.link = headerValues[1];
      menuItem.icon = headerValues[2];
      if (headerValues[3] != "0") {
        menuItem.child = [];
      }
     
      var childs = h.split(":");
      if (childs.length == 1) {
        //if length == 1 means that no have child beacause split() returns 1 item also if no split was done
        deserializedMenu.push(menuItem);
        return;
      } 

      childs.forEach(c => {
        var childValues = c.split("|");       
        menuItem.child?.push({
          title: childValues[0],
          link: childValues[1],
          icon: childValues[2]
        })
      });
      deserializedMenu.push(menuItem);
    })
    this.menu = deserializedMenu;
  }
}
