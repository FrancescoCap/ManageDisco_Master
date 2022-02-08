import { OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { Component } from '@angular/core';
import { catchError,pipe } from 'rxjs';
import { ApiCaller } from './api/api';
import { ChildMenu, HeaderMenu } from './model/models';
import { ModalService } from './service/modal.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'ManageDiscoClient';

  menu?: HeaderMenu[];

  constructor(private _api: ApiCaller) { }

  ngOnInit(): void {
    this.getMenu();
  }

  getMenu() {
    this._api.getMenu().pipe(
      catchError(err => {
        return err;
      })).subscribe(data => {       
        this.menu = data as HeaderMenu[];
      })
  }

}
