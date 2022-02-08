import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ApiCaller } from '../../api/api';
import { ModalService } from '../../service/modal.service';
import SwiperCore, {Autoplay, Navigation, EffectCoverflow, Scrollbar, Pagination } from "swiper";
import { catchError } from 'rxjs';
import { HomeInfo } from '../../model/models';
import { Router } from '@angular/router';

SwiperCore.use([EffectCoverflow, Autoplay, Navigation, Scrollbar, Pagination]);

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss', '../../app.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnInit {

  homeInfo: HomeInfo = { events: []};

  constructor(private _modalService: ModalService,
    private _api: ApiCaller,
    private _router: Router) { }

  ngOnInit(): void {
    this.initData();
  }

  initData() {
    this._api.getHomeInfo().pipe(
      catchError(err => {
        this._modalService.showErrorModal(err.message);
        return err;
      })).subscribe((data: any): any => {
        this.homeInfo.events = data.events;
        console.log(this.homeInfo.events)
      })
  }

  getEventInfo(id: any) {
    this._router.navigateByUrl(`Events/Details?eventId=${id}`);
  }

}
