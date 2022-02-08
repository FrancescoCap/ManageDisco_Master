import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { NavigationLabel, Reservation, UserInfoView } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;
  user?: UserInfoView;
  userReservation?: Reservation[] = [];

  userEdit = false;

  navigationLabel: NavigationLabel[] = [
    {label:"I miei dati", isActive: true, link:"#myData"},
    {label:"Le mie prenotazioni", isActive: false, link:"#myReservation"}
  ]

  constructor(private _api: ApiCaller,
        private _modalService:ModalService  ) { }

  ngOnInit(): void {
    this.initData();
  }

  ngAfterViewInit() {    
    this._modalService.setContainer(this.modalContainer!);
  }

  initData() {

    const calls: Observable<any>[] = [
      this._api.getUserInfo(),
      this._api.getUserReservation()
    ]

   forkJoin(calls).pipe(
      catchError(err => {
        this._modalService.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {    
        this.user = data[0];
        this.userReservation = data[1];
      })
  }

  getLabelColor(menuItem:any) {
    var color: string = menuItem.isActive == true ? "#fdfdfd":"#e9e9e9"; //Bianco : Grigio
    return color;
  }

  onNavigationLabelClick(item:any) {
    this.navigationLabel.forEach(x => x.isActive = false);
    this.navigationLabel.find(x => x.label == item.label)!.isActive = true;
  }

  userInfoEnableEdit() {
    this.userEdit = true;
  }

  confirmUserEdit() {
    var userData = { email: this.user?.userEmail, name: this.user?.userName, surname: this.user?.userSurname };

    this._api.putUserInfo(userData).pipe(
      catchError(err => {
        this._modalService.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.userEdit = false;
        this.initData();        
      })
  }

  changePr() {
    var modalViews: ModalViewGroup[] = [
      { type: ModalModelEnum.TextBox, viewItems: [{viewId: "txtPrCode", referenceId: "prCode", label:"Codice pr"}]}
    ]

    this._modalService.showAddModal(this.onPrChanged, "Cambia PR", modalViews);
  }

  onPrChanged = (info: any): void => {
    this._api.putChangePrCustomer(info.get("prCode")).pipe(
      catchError(err => {
        this._modalService.showErrorModal(err.message);
        return err;
      })).subscribe((response: any) => {
        if (response != null)
          this._modalService.showErrorModal(response);
        else
          this.initData();
      })
  }
}
