import { ViewChild } from '@angular/core';
import { ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError, forkJoin } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalViewGroup } from '../../components/modal/modal.model';
import { GeneralMethods } from '../../helper/general';
import { ModalType, ProductShopHeader } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-shop',
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.scss', '../../app.component.scss']
})
export class ShopComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  productsShop?: ProductShopHeader[] = [];
  
  isMobileView = false;
  isTabletView = false;
  isLoading = false;
  userIsAdministrator = false;

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _user: UserService,
    private _generalService:GeneralService  ) { }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
    this.isTabletView = this._generalService.isTabletView();
    this.userIsAdministrator = this._user.userIsAdminstrator();
    this.initData();
  }

  ngAfterViewInit() {
    if (this.modalContainer != null)
      this._api.setModalContainer(this.modalContainer);
  }

  initData() {
    this.isLoading = true;
    this._api.getShop().subscribe((data: any) => {
      this.productsShop = data;
      this.isLoading = false;
    })
  }

  openNewProductModal() {
   
    forkJoin([
      this.getProducts(true),
      this.getProductsShopType()]).subscribe((data: any[]) => {

      var inputs: ModalViewGroup[] = [
        {
          type: ModalModelEnum.TextBox, viewItems: [
            { referenceId: "productName", viewId: "txtProdName", label: "Nome prodotto" },
            { referenceId: "productPrice", viewId: "txtProdPrice", label: "Punti prodotto" }
          ]
        },
        {
          type: ModalModelEnum.Table, multiSelect:true, txtTableQuantity:true, viewItems: [
            { referenceId: "productId", viewId: "drpProducts", label: "Prodotto", list:data[0] }
          ]
        },
        {
          type: ModalModelEnum.Dropdown, viewItems: [
            { referenceId: "productsType", viewId: "drpProductsType", label: "Tipo prodotto", list: data[1] }
          ]
        },
        {
          type: ModalModelEnum.Textarea, viewItems: [
            { label: 'Descrizione prodotto', referenceId: 'productDescription', viewId: 'txtProdDescription' }
          ]
        }, {
          type: ModalModelEnum.ImageBox, viewItems: [
            {label: "Immagine", referenceId:"imgProduct", viewId:"imgShopProduct"}
          ]
        }
      ];

      this._modal.showAddModal(this.onProductAdded, "NUOVO PRODOTTO", inputs, ModalType.NEW_RESERVATION);
    });    
    
  }

  getProducts(isShopProduct:boolean) {
    return this._api.getProducts(undefined,isShopProduct);
  }

  getProductsShopType() {
    return this._api.getProductShopType();
  }

  onProductAdded = (response: any) => {
    var product: ProductShopHeader = {
      productShopHeaderName: response.get("productName"),
      productShopHeaderPrice: response.get("productPrice"),
      productShopHeaderDescription: response.get("productDescription"),
      productShopTypeId: response.get("productsType"),
      productShopBase64Image: response.get("imgProduct")
    }
    product.rows = [];
    response.get("txtTableQuantity").forEach((x:any, y:any) => {
      product.rows?.push({ productId: x.split(":")[0], productShopRowQuantity: x.split(":")[1]});
    })
  
    this.addProduct(product);
  }

  addProduct(data:ProductShopHeader) {
    this.isLoading = true;

    if (this.productsShop != null) {
      this._api.postShop(data).subscribe((data: any) => {
        this.isLoading = false;        
        this.initData();
      })
    }

  }

  purchase(productShopId:any) {
    if (!this._user.userIsAuthenticated()) {
      this._modal.showAddModal(this.onLogin, "LOGIN", GeneralMethods.getLoginModalViews());
    }

    this.isLoading = true;
    this._api.purchaseProduct(productShopId).pipe(
      catchError(err => {
        this.isLoading = false;
        console.log("error")
        return err;
      }))
      .subscribe((data: any) => {
        console.log("OK")
      this.isLoading = false;
      this._modal.showErrorOrMessageModal("Complimenti! Hai acquisito il premio", "ACQUISTO", true);
    })
    
  }

  onLogin = (response: any): void => {

  }

}
