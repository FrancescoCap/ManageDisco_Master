import { ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { Product, Warehouse } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.scss', '../../app.component.scss']
})
export class WarehouseComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  warehouse?: Warehouse[];
  productIdSelected?: any;
  isMobileView = false;
  isTabletView = false;
  isLoading = false;

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _generalService:GeneralService  ) { }

  ngOnInit(): void {
    this.initData();
    this.isMobileView = this._generalService.isMobileView();
    this.isTabletView = this._generalService.isTabletView();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initData() {
    this.isLoading = true;
    this._api.getWarehouse().pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: any) => {
        this.warehouse = data;
        this.isLoading = false;
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
    this.isLoading = true;
    var qty: Warehouse = {
      warehouseQuantity: info.get("productQuantity"),
      productId: this.productIdSelected
    }

    this._api.putWarehouseQuantity(qty).pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: any) => {
        this.isLoading = false;
        this._modal.hideModal();
        this.initData();
      })
  }

  addProductToCatalog() {
    this.isLoading = true;
    this._api.getCatalogs().pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: any) => {
        //inizializzo la modal
        var modelView: ModalViewGroup[] = [];
        modelView.push({
          type: ModalModelEnum.TextBox, viewItems: [{
            label: "Nome prodotto", viewId: "txtProductName", referenceId: "productName"
          },
          {
            label: "Prezzo", viewId: "txtProductPrice", referenceId: "productPrice"
          }]
        });
        modelView.push({
          type: ModalModelEnum.Dropdown, viewItems: [{
            label: "Catalogo", viewId: "drpCatalog", referenceId: "catalogId", list: data.catalog
          }]
        });
        this.isLoading = false;
        
        this._modal.showAddModal(this.onProductAdded, "Nuovo prodotto", modelView);
      });
  }

  onProductAdded = (info: any): void => {
    this.isLoading = true;
    var product: Product = {
      productName: info.get("productName"),
      productPrice: info.get("productPrice"),
      catalogId: info.get("catalogId")
    }

    this._api.postProduct(product).pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe(() => {
        this.isLoading = false;
        this._modal.hideModal();
        this.initData();
      })
  }
}
