import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { catchError, forkJoin, Observable, Subject } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { TableViewDataModel } from '../../components/tableview/tableview.model';
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

  reservationTableDate: TableViewDataModel = { headers: [], rows: [] };

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

  isLoading?: boolean = false;
  loadingSubject?: Subject<boolean> = new Subject<boolean>();

  userIsAdministrator = false;
  userIsCustomer = false;

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
  onEventChangeCaller: Subject<number> = new Subject<number>();

  constructor(private _api: ApiCaller,
    private _modalService: ModalService,
    private _user: UserService,
    private _generalService: GeneralService  ) { }

  ngOnInit(): void {
    this.initFlags();
  }

  ngAfterContentInit() {
    this._modalService.setContainer(this.modalContainer!);
    
  }

  initFlags() {
    this.userIsAdministrator = this._user.userIsAdminstrator();
    this.userIsCustomer = this._user.userIsCustomer();
    this.isMobileView = this._generalService.isMobileView();
    this.isTabletView = this._generalService.isTabletView();
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
        //this.getEvents();
        break;
      case this.PERMISSION_VIEW_INDEX:
        //this.getPermissionActions();
        break;
    }
      this.registrationRequest = { email: "", password: "", name: "", surname: "", username: "", phoneNumber: "", role: this.registrationRequest?.role, gender:"Male" };
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

  onCollaboratorRegistered(event: any) {
    this._api.register(this.registrationRequest).pipe(
      catchError(err => {
        return err;
      })).subscribe((data: any) => {
        if (data.message.includes("success"))
          this._modalService.showErrorOrMessageModal("Registrazione avvenuta con successo", "ESITO", true);

        this.registrationRequest = { name: "", surname: "", email: "", password: "", role: 0, phoneNumber: "", username: "", gender: "Male" };
      })
  }

  eventStatChange(eventId:number) {
    this.onEventChangeCaller.next(eventId);
  }

  //callback to child view component
  onChildLoading(isLoading: boolean) {
    this.loadingSubject?.next(isLoading);
  }

}
