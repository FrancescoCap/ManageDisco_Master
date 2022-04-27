import { OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { Component } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { catchError,pipe } from 'rxjs';
import { ApiCaller } from './api/api';
import { client_URL } from './app.module';
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

  constructor(private _api: ApiCaller,
    private _user: UserService,
    private _generalService:GeneralService  ) { }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
    this.isAuthenticated = this._user.userIsAuthenticated();
    this.getFooterAndHeader();      
  }

  getFooterAndHeader() {
    var calls: Observable<any>[] = [
      this._api.getHomeInfo(),
      this._api.getMenu()
    ]


    forkJoin(calls).subscribe((data: any) => {
      this.homeInfo = data[0]
      this.menu = data[1] as HeaderMenu[];
      this.instagramContactTypeId = this.homeInfo!.contacts?.find(x => x.contactTypeDescription?.includes("Instagram"))?.contactTypeId;
      this.facebookContactTypeId = this.homeInfo!.contacts?.find(x => x.contactTypeDescription?.includes("Facebook"))?.contactTypeId;     
    });

  }

  logout() {
    this._api.logout().subscribe(() => {
        document.location.href = client_URL + "/Login";
      })
  }

}
