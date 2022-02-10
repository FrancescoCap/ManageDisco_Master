import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { catchError, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { Contact, ContactType, HomeInfo, HomePhotoPost, HomePhotoPut, PhotoType } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-home-settings',
  templateUrl: './home-settings.component.html',
  styleUrls: ['./home-settings.component.scss', '../../app.component.scss']
})
export class HomeSettingsComponent implements OnInit {

  readonly HOME_GALLERY_MAIN = "Home_Galleria";
  readonly HOME_GALLERY_MOMENTS = "Home_Momenti";
  /*READ DATA*/
  homeInfo?: HomeInfo;
  contact?: Contact[];
  contactType?: ContactType[];


  homePhotoPost: HomePhotoPost[] = [];
  homePhotoPut?: HomePhotoPut;

  homePhotoMainGalleryInput?: any;
  homePhotoGalleryMomentsInput?: any;

  gallery_main?: HomeInfo;
  gallery_moments?: HomeInfo;

  showAddContactTemplate = false;
  newContact: Contact = { contactTypeId: 0 };  

  settingsSlots: boolean[] = [
    false,
    false,
    false
  ]

  constructor(private _api: ApiCaller,
        private _modal:ModalService) { }

  ngOnInit(): void {
    this.initData();
  }

  initData() {
    const calls: Observable<any>[] = [
      this._api.getHomeInfo(),
      this._api.getContact(),
      this._api.getContactTypes()
    ];

    forkJoin(calls).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {       
        this.homeInfo = data[0];
        this.contact = data[1];
        this.contactType = data[2];

        this.homePhotoMainGalleryInput = Array(this.homeInfo?.photoType?.find(x => x.photoTypeDescription?.includes(this.HOME_GALLERY_MAIN))?.photoTypeMaxNumber);
        this.gallery_main = { homePhoto: this.homeInfo!.homePhoto!.filter(x => x.photoTypeDescription == this.HOME_GALLERY_MAIN)! };
        //  .homePhoto = this.homeInfo?.photoType?.filter(x => x.photoTypeDescription?.includes(this.HOME_GALLERY_MAIN))!;
        
        this.homePhotoGalleryMomentsInput = Array(this.homeInfo?.photoType?.find(x => x.photoTypeDescription?.includes(this.HOME_GALLERY_MOMENTS))?.photoTypeMaxNumber);
        this.gallery_moments = { homePhoto: this.homeInfo!.homePhoto!.filter(x => x.photoTypeDescription == this.HOME_GALLERY_MOMENTS)! };

        this.showAddContactTemplate = false;
        this.newContact = { contactTypeId: 0 };
        this.homePhotoPost = [];
      })
  }

  onMainGalleryFileChange(event: any, homePhotoPath:string) {
    var typePhotoId = this.homeInfo?.photoType?.find(x => x.photoTypeDescription?.includes(this.HOME_GALLERY_MAIN))?.photoTypeId;

    this.homePhotoPost.push({
      photoTypeId: typePhotoId,
      homePhotoBase64: "",
      photoName: homePhotoPath
    });

    //qui gli passo sempre l'index dell'ultima foto aggiunta così da associare il codice base64 estrapolato dalla funzione, sempre all'ultima immagine
    this.convertFileToBase64(event.target.files[0], this.HOME_GALLERY_MAIN, this.homePhotoPost.length);
  }

  onGalleryMomentsFileChange(event: any, homePhotoPath: string) {
    var typePhotoId = this.homeInfo?.photoType?.find(x => x.photoTypeDescription?.includes(this.HOME_GALLERY_MOMENTS))?.photoTypeId;

    this.homePhotoPost.push({
      photoTypeId: typePhotoId,
      homePhotoBase64: "",
      photoName: homePhotoPath
    });

    //qui gli passo sempre l'index dell'ultima foto aggiunta così da associare il codice base64 estrapolato dalla funzione, sempre all'ultima immagine
    this.convertFileToBase64(event.target.files[0], this.HOME_GALLERY_MAIN, this.homePhotoPost.length);
  }

  convertFileToBase64(blob:Blob, group:any, index?:number) {

    var fileReader = new FileReader();
    fileReader.onload = (event: any) => {
      this.homePhotoPost[index!-1].homePhotoBase64 = event.target.result;
    }
    fileReader.readAsDataURL(blob);
  }

  saveHomePhotos() {
    this._api.postHomePhoto(this.homePhotoPost).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe(() => {
        this.initData();
      })
  }

  saveContact() {

  }

  addContact() {
    this.showAddContactTemplate = true;
  }

  confirmNewContact() {
    this._api.postContact(this.newContact).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.initData();
      })
  }

}
