import { Output, ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError, toArray } from 'rxjs';
import { concatMap, forkJoin, of } from 'rxjs';
import { ApiCaller } from '../../../api/api';
import { TableViewDataModel } from '../../../components/tableview/tableview.model';
import { GeneralMethods } from '../../../helper/general';
import { NavigationLabel, UserInfoPut, UserInfoView, UserProduct } from '../../../model/models';
import { ModalService } from '../../../service/modal.service';
import { UserService } from '../../../service/user.service';
import { maxTablePageRows } from '../../../app.module';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'app-user-info',
  templateUrl: './user-info.component.html',
  styleUrls: ['./user-info.component.scss', '../../../app.component.scss']
})
export class UserInfoComponent implements OnInit {

  readonly VIEW_USER_DATA = 0;
  readonly VIEW_USER_AWARDS = 1;
  readonly PHONE_NUMBER_WARNING = "Il numero di telefono non è stato confermato.\nNon sarà per tanto possibile ricevere informazioni da noi, omaggi inclusi."
  rowsPerPage = maxTablePageRows;

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;
  @Output("loading") onLoading: EventEmitter<boolean> = new EventEmitter<boolean>();

  activeSection: number = this.VIEW_USER_DATA;

  sections: NavigationLabel[] = [
    { label: "DATI", index: this.VIEW_USER_DATA, isActive: false, child: [] },
    { label: "PREMI", index: this.VIEW_USER_AWARDS, isActive: false, child: [] }];

  user?: UserInfoView;
  userAwardsList?: UserProduct[];
  userAwardsTableData: TableViewDataModel = {
    headers: [
      { value: "Nome"},
      { value: "Descrizione"},
      { value: "Codice"},
      { value: "Valido"}
    ], rows: []
  }

  userIsCustomer = false;
  isLoading = false;
  enableEdit = false;

  userEditInfo: UserInfoPut = { name: "", surname: "", email:"", phoneNumber:""};

  constructor(private _api: ApiCaller,
    private _userService: UserService,
    private _modal:ModalService) { }

  ngOnInit(): void {
    this.initFlags();
    this.initData();
  }
  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initFlags() {
    this.sections.find(x => x.index == this.VIEW_USER_DATA)!.isActive = true;
  }

  initData() {
    this.getUserInfo();
  }

  getUserInfo() {
    this.isLoading = true;
    this.onLoading.emit(this.isLoading);

    var calls = [
      this._api.getUserInfo(),
      this._api.getUserAwards()
    ]
    //Va bene usare anche la forkjoin perchè le chiamate non sono strettamente collegate dal risultato dell'una o dell'altra, basta che si attendano
    forkJoin(calls).subscribe((data: any) => {
      this.user = data[0];
      this.userAwardsList = data[1];
      this.initUserEditObject();
      //TODO BAD WAY
      this.userIsCustomer = data[0].isCustomer;
      this.setDataTableView();

      this.isLoading = false;
      this.onLoading.emit(this.isLoading);
    })
   
  } 

  setDataTableView() {
    this.userAwardsList?.forEach((x, y) => {
      this.userAwardsTableData.rows.push({
        cells: [{
          value: x.productShopHeader?.productShopHeaderName
        },
        {
          value: x.productShopHeader?.productShopHeaderDescription
        }, {
          value: x.userProductCode
        }, {
          value: "", icon: [{
            isImage: true, imagePath: x.userProductUsed ? "../../../assets/circle_red_full.png" : "../../../assets/circle_green_full.png"
          }]
        }]
      })
    })
  }

  onSectionClick(index?: number) {
    this.resetSectionSelection();
    this.sections.find(x => x.index == index)!.isActive = true;
    this.activeSection = index!;
  }

  resetSectionSelection() {
    var activeTab = this.sections.find(x => x.isActive == true);
    if (activeTab != null)
      this.sections.find(x => x.isActive == true)!.isActive = false;
  }

  onChangePrClick() {
    this._modal.showAddModal(this.onPrChanged, "CAMBIA PR", GeneralMethods.getChangePrModalViews());
  }

  onPrChanged = (code: any):any =>  {
    //don't need to reload user awards because this context doesn't include something on userProduct
    var calls = of(this._api.putChangePrCustomer(code.get("prCode")), this._api.getUserInfo())

    calls.pipe().pipe(
      concatMap((values: any) => { return values }),
      toArray()
    ).subscribe((data: any) => {
      this.user = data[1];
      this._modal.hideModal();
    })
  }

  onEditInfoClick() {
    this.enableEdit = true;
  }

  initUserEditObject() {
    this.userEditInfo = { email: this.user?.userEmail, name: this.user?.userName, surname: this.user?.userSurname, phoneNumber: this.user?.userPhoneNumber };
  }

  onConfirmEditing() {
    this.isLoading = true;
    this.onLoading.emit(this.isLoading);

    var calls = of(this._api.putUserInfo(this.userEditInfo), this._api.getUserInfo());

    calls.pipe(
      concatMap((values: any) => { return values; }),
      toArray(),
      catchError(err => {
        this.isLoading = false;
        return err;
      })
    ).subscribe((data: any) => {
      this.user = data[1];
      this.enableEdit = false;
      this.isLoading = false;
      this.onLoading.emit(this.isLoading);
    })
  }

  onPhoneNumberWarningClick() {
    this._modal.showErrorOrMessageModal(this.PHONE_NUMBER_WARNING,"ATTENZIONE");
  }
}
