import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { ApiCaller } from '../../api/api';
import { ModalService } from '../../service/modal.service';
import SwiperCore, {Autoplay, Navigation, EffectCoverflow, Scrollbar, Pagination } from "swiper";
import { catchError, forkJoin, Observable } from 'rxjs';
import { Contact, HomeInfo } from '../../model/models';
import { Router } from '@angular/router';

SwiperCore.use([EffectCoverflow, Autoplay, Navigation, Scrollbar, Pagination]);

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss', '../../app.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnInit {

  homeInfo: HomeInfo = { events: [], homePhoto: [], photoType: [] };
  contacts: Contact[] = [];

  photoGalleryMain?: HomeInfo;
  photoGalleryMoments?: HomeInfo;

  instagramContactTypeId?: number;
  facebookContactTypeId?: number;

  readonly HOME_GALLERY_MAIN = "Home_Galleria";
  readonly HOME_GALLERY_MOMENTS = "Home_Momenti";

  constructor(private _modalService: ModalService,
    private _api: ApiCaller,
    private _router: Router) { }

  ngOnInit(): void {
    this.initData();
  }

  initData() {
    const calls: Observable<any>[] = [
      this._api.getHomeInfo()
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        this._modalService.showErrorModal(err.message);
        return err;
      })).subscribe((data: any): any => {
        this.homeInfo = data[0];
        //Pulisco la galleria foto dalle immagini vuote
        this.homeInfo.homePhoto = this.homeInfo.homePhoto?.filter(x => x.homePhotoPath != "no_image.webp");

        this.photoGalleryMain = { homePhoto: this.homeInfo.homePhoto?.filter(x => x.photoTypeDescription == this.HOME_GALLERY_MAIN)! };       
        this.photoGalleryMoments = { homePhoto: this.homeInfo.homePhoto?.filter(x => x.photoTypeDescription == this.HOME_GALLERY_MOMENTS)! };

        this.instagramContactTypeId = this.homeInfo.contacts?.find(x => x.contactTypeDescription?.includes("Instagram"))?.contactTypeId;
        this.facebookContactTypeId = this.homeInfo.contacts?.find(x => x.contactTypeDescription?.includes("Facebook"))?.contactTypeId;
      })
  }

  getEventInfo(id: any) {
    this._router.navigateByUrl(`Events/Details?eventId=${id}`);
  }

}
