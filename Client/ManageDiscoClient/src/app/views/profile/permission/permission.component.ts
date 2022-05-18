import { Component, OnInit } from '@angular/core';
import { catchError, concatMap, of, toArray } from 'rxjs';
import { ApiCaller } from '../../../api/api';
import { TableViewDataModel } from '../../../components/tableview/tableview.model';
import { UserPermissionPut, UserPermissionTable } from '../../../model/models';

@Component({
  selector: 'app-permission',
  templateUrl: './permission.component.html',
  styleUrls: ['./permission.component.scss', '../../../app.component.scss']
})
export class PermissionComponent implements OnInit {

  userPermission?: UserPermissionTable;
  permissionTableList: TableViewDataModel = { headers: [{ value: "Utente" }], rows: [] }
  maxTablePageRows = 10;
  isMobileTemplate = false;
  isLoading = false;

  constructor(private _api: ApiCaller) { }

  ngOnInit(): void {
    this.initData();
  }

  initData() {
    this.getPermissionActions();
  }

  getPermissionActions() {
    this.isLoading = true;
    this._api.getUserPermission()
      .subscribe((data: UserPermissionTable) => {
        this.userPermission = data;
        this.setDataForTableView();
        this.isLoading = false;
      })
  }

  reinitPermissionTableList() {
    this.permissionTableList = { headers: [{ value: "Utente" }], rows: [] };
  }

  setDataForTableView() {

    if (this.permissionTableList.headers.length > 0)
      this.reinitPermissionTableList();

    this.userPermission?.userPermissionTableHeaderCol?.forEach((headers: any) => {
      this.permissionTableList.headers.push({ value: headers });
    });

    this.userPermission?.rows?.forEach((x, y) => {
      this.permissionTableList.rows.push({ cells: [{ value: x.user }] });

      this.userPermission!.rows![y]!.userPermissionTableCell!.forEach((p, i) => {
        
        this.permissionTableList.rows[y].cells?.push({
          icon: [{
            class: '',
            isToShow: true,
            isImage: true,
            imagePath: p.permissionState ? '../../../../assets/circle-check-solid.svg' : '../../../../assets/circle-regular.svg',
            referenceId: `chbPerm_${p.permissionId}|${p.userId}`,
            onClickCallback: this.onPermissionChangeState
          }]
        })
      })          
    })
  }

  onPermissionChangeState = (info: any): void => {
    this.isLoading = true;
    var splitInfo = info.split("|");
    var putPermission: UserPermissionPut = {
      permissionId: splitInfo[0],
      userId: splitInfo[1]
    }

    var calls = of(this._api.putUserPermission(putPermission), this._api.getUserPermission());    
   
    calls.pipe(
      concatMap(vals => { return vals }),
      toArray(),
      catchError(err => { return err;})
    ).subscribe((data:any) => {
      this.userPermission = data[1];
      this.setDataForTableView();
      this.isLoading = false;
    })
  }
}
