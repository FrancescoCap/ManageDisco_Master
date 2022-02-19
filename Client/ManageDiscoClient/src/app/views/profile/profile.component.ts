import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { NavigationLabel, RegistrationRequest, Reservation, UserInfoView } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss', '../../app.component.scss']
})
export class ProfileComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;
  user?: UserInfoView;
  userReservation?: Reservation[] = [];
  registrationRequest?: RegistrationRequest;

  userEdit = false;

  PROFILE_VIEW_INDEX = 0;
  RESERVATION_VIEW_INDEX = 1;
  COLLABORATORS_VIEW_INDEX = 2;

  pageViews: boolean[] = [
    true,
    false,
    false
  ]

  isLoading = false;

  navigationLabel: NavigationLabel[] = [
    {label:"I miei dati", isActive: true, id: "Profile", index: 0},
    {label:"Le mie prenotazioni", isActive: false, id:"Reservations", index:1},
    {label:"Nuovo Collaboratore", isActive: false, id:"Collaborators", index:2}
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
    this.isLoading = true;
    const calls: Observable<any>[] = [
      this._api.getUserInfo(),
      this._api.getUserReservation()
    ]

   forkJoin(calls).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {    
        this.user = data[0];
        this.userReservation = data[1];

        this.isLoading = false;
      })
  }

  getLabelColor(menuItem:any) {
    var color: string = this.pageViews[menuItem] ? "#fdfdfd":"#e9e9e9"; //Bianco : Grigio
    return color;
  }

  onNavigationLabelClick(item: any) {
    this.pageViews.forEach((x,y) => {
      this.pageViews[y] = false;
    })
    this.pageViews[item] = true;

    if (item == this.COLLABORATORS_VIEW_INDEX)
      this.registrationRequest = { email: "", password: "", name: "", surname: "", username: "", role:0 };

  }

  userInfoEnableEdit() {
    this.userEdit = true;
  }

  confirmUserEdit() {
    var userData = { email: this.user?.userEmail, name: this.user?.userName, surname: this.user?.userSurname };

    this._api.putUserInfo(userData).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
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
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((response: any) => {
        if (response != null)
          this._modalService.showErrorOrMessageModal(response);
        else
          this.initData();
      })
  }

  onCollaboratorRegistered(event: any) {
    this.isLoading = true;
    this._api.register(this.registrationRequest).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        if (data.message.includes("success"))
          this._modalService.showErrorOrMessageModal("Registrazione avvenuta con successo", "ESITO");

        this.initData();
        this.registrationRequest = { name: "", surname:"", email:"", password:"", role:0, username:""}
        this.isLoading = false;
      })
  }
}
