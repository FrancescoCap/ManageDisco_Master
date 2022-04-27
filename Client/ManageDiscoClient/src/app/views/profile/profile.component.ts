import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { EventParty, UserPermissionPut, NavigationLabel, RegistrationRequest, Reservation, Role, Statistics, User, UserInfoView, UserPermission, UserPermissionCell, UserPermissionTable, UserProduct } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

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
  events?: EventParty[];
  userPermission?: UserPermissionTable;

  selectedEvent?: any = 0;

  statistics?: Statistics;
  freeEntranceVisibleItems: number = 10;
  freeEntrancePagesCollection: number[] = [];
  freeEntranceSelectedPage: number = 1;

  userEdit = false;

  PROFILE_VIEW_INDEX = 0;
  RESERVATION_VIEW_INDEX = 1;
  COLLABORATORS_VIEW_INDEX = 2;
  STATISTICS_VIEW_INDEX = 3;
  PERMISSION_VIEW_INDEX = 4;

  /*CHILD LABEL INDEX*/
  PROFILE_MYDATA_VIEW_INDEX = 0;
  PROFILE_AWARDS_VIEW_INDEX = 1;

  pageViews: boolean[] = [
    true,
    false,
    false
  ]

  isLoading = false;
  userIsAdministrator = false;

  navigationLabel: NavigationLabel[] = [
    {
      label: "I miei dati", isActive: true, id: "Profile", index: this.PROFILE_VIEW_INDEX, child: [
        { label: "I MIEI DATI", isActive: true, index: this.PROFILE_MYDATA_VIEW_INDEX },
        { label: "PREMI", isActive: false, index: this.PROFILE_AWARDS_VIEW_INDEX }
      ]
    },
    { label: "Le mie prenotazioni", isActive: false, id: "Reservations", index: this.RESERVATION_VIEW_INDEX, child:[]},
    { label: "Nuovo Collaboratore", isActive: false, id: "Collaborators", index: this.COLLABORATORS_VIEW_INDEX, child: []},
    { label: "Statistiche", isActive: false, id: "Collaborators", index: this.STATISTICS_VIEW_INDEX, child: []},
    { label: "Permessi", isActive: false, id: "Permissions", index: this.PERMISSION_VIEW_INDEX, child: []}
  ]

  userProducts?: UserProduct[];

  roles: Role[] = [];
  isMobileView = false;
  isTabletView = false;

  constructor(private _api: ApiCaller,
    private _modalService: ModalService,
    private _user: UserService,
    private _generalService: GeneralService  ) { }

  ngOnInit(): void {
    this.initData();
    this.userIsAdministrator = this._user.userIsAdminstrator();
  }

  ngAfterViewInit() {    
    this._modalService.setContainer(this.modalContainer!);
    this.isMobileView = this._generalService.isMobileView();
    this.isTabletView = this._generalService.isTabletView();
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

    switch (item) {
      case this.COLLABORATORS_VIEW_INDEX:
        if (this.userIsAdministrator)
          this.getAvaiableRoles();
        break;
      case this.STATISTICS_VIEW_INDEX:
        this.getEvents();
        break;
      case this.PERMISSION_VIEW_INDEX:
        this.getPermissionActions();
        break;
    }
      this.registrationRequest = { email: "", password: "", name: "", surname: "", username: "", phoneNumber: "", role: this.registrationRequest?.role, gender:"Male" };
  }

  onChildLabelClick(lblMasterIndex: any, lblChildIndex: any) {
    this.navigationLabel[lblMasterIndex].child.forEach((x, y) => {
      x.isActive = false;
    })
    this.navigationLabel[lblMasterIndex].child[lblChildIndex].isActive = true;
    this.getChildData(lblChildIndex);
  }

  getChildData(index: any) {
    switch (index) {
      case this.PROFILE_AWARDS_VIEW_INDEX:
        this.getUserAwards();
        break;
    }
  }

  getUserAwards() {
    this._api.getUserAwards().subscribe((data: any) => {
      this.userProducts = data;
    })
  }

  getAvaiableRoles() {
    this._api.getRoles().pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message, "ERRORE");
        return err;
      })).subscribe((data: any) => {
        this.roles = data;
      })
  }

  userInfoEnableEdit() {
    this.userEdit = true;
  }

  confirmUserEdit() {
    var userData = { email: this.user?.userEmail, name: this.user?.userName, surname: this.user?.userSurname, phoneNumber: this.user?.userPhoneNumber };

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
        return err;
      })).subscribe((data: any) => {
        if (data.message.includes("success"))
          this._modalService.showErrorOrMessageModal("Registrazione avvenuta con successo", "ESITO", true);

        this.initData();
        this.registrationRequest = { name: "", surname:"", email:"", password:"", role:0, phoneNumber:"", username:"", gender: "Male"}
        this.isLoading = false;
      })
  }

  onPhoneNumberClicked() {
    this._modalService.showErrorOrMessageModal("Non hai ancora confermato il numero di telefono.\nFino a che non sarà completata l'operazione non sarà possibile " +
      "ricevere comunicazioni da noi, inclusi gli omaggi", "AVVISO", true);
  }

  getEvents() {
    this._api.getEvents().pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message, "ERRORE", false);
        return err;
      })).subscribe((data: any) => {
        this.events = data.events;
      })
  }

  getPermissionActions() {
    var calls: Observable<any>[] = [
      this._api.getUserPermission()
    ]

    forkJoin(calls).subscribe((data: any) => {
      this.userPermission = data[0];
    })

  }

  eventStatChange() {
    this.getStatistics();
  }

  getStatistics() {
    this.isLoading = true;
    this._api.getStatisticsForEvent(this.selectedEvent).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message, "ERRORE");
        return err;
      })).subscribe((data: any) => {
        this.statistics = data;
        this.setFreeEntrancePage();
        this.isLoading = false;
      })
  }

  setFreeEntrancePage() {
    if (this.freeEntrancePagesCollection != null && this.freeEntrancePagesCollection.length > 0)
      this.freeEntrancePagesCollection = [];
    if (this.statistics != null && this.statistics.freeEntrance != null) {
      var page = 1;
      for (var i = 0; i < this.statistics?.freeEntrance?.couponList!.length; i++) {
        if ((i%10) == 0) {
          this.freeEntrancePagesCollection.push(page);
          page++;
        }
      }
    }
  }

  freeEntranceChangePage(text:any) {
    this.freeEntranceVisibleItems = 10 * text;
    this.freeEntranceSelectedPage = text;
  }

  onPermissionStateChange(memberId?:string, permId?:number) {

    var putPermission: UserPermissionPut = {
      permissionId: permId,
      userId: memberId
    }

    this._api.putUserPermission(putPermission)
      .subscribe(() => {
        this.getPermissionActions();
      })
  }
}
