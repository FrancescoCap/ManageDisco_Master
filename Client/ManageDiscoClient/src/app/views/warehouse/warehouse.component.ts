import { ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { Warehouse } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.scss','../../app.component.scss']
})
export class WarehouseComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  warehouse?: Warehouse[];
  productIdSelected?: any;

  constructor(private _api: ApiCaller,
      private _modal:ModalService) { }

  ngOnInit(): void {
    this.initData();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initData() {
    this._api.getWarehouse().pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.warehouse = data;
      })
  }

  addQuantity(productId: any) {
    var modalViews: ModalViewGroup[] = [
      {
        type: ModalModelEnum.TextBox, viewItems: [{
          viewId: "txtQuantity", referenceId: "productQuantity", label: "Quantità in aggiunta"
        }]
      }
    ];
    var productName = this.warehouse?.find(x => x.productId == productId)?.productName;
    this.productIdSelected = productId;
    this._modal.showAddModal(this.onQuantityAdded, `Rifornimento per ${productName}`, modalViews);
  }

  onQuantityAdded = (info: any): void => {
    var qty: Warehouse = {
      warehouseQuantity: info.get("productQuantity"),
      productId: this.productIdSelected
    }

    this._api.putWarehouseQuantity(qty).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe((data: any) => {

        this.initData();
      })
  }
}
