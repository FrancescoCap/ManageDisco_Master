import { ViewChild, ViewContainerRef } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { catchError, last } from 'rxjs';
import { forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalModelEnum, ModalModelList, ModalTextBoxList, ModalViewGroup } from '../../components/modal/modal.model';
import { Catalog, ModalType, Product } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.scss', '../../app.component.scss']
})
export class ProductComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  products: Product[] = [];
  productsFiltered: Product[] = [];
  catalogs: Catalog[] = [];

  showShieldBack: Map<any, boolean> = new Map();
  shieldText: string = "";

  modalViews?: ModalViewGroup[];
  selectedCatalog = 0;

  /*MODAL NEW PRODUCT FLAG AND VARIABLES*/
  title: string = "NUOVO PRODOTTO";
  visibility = false;
  textBox: ModalTextBoxList[] = [{ id: "productName", label: "Nome" }, { id: "productPrice", label: "Prezzo" }];
  dropDown: ModalModelList[] = [];
  modalType = ModalType.NEW_PRODUCT;
  

  constructor(private _api: ApiCaller,
      private _modal:ModalService) { }

  ngOnInit(): void {
    this.initData();
  }

  ngAfterViewInit() {
    this._api.setModalContainer(this.modalContainer!);
  }

  initData() {
    const calls: Observable<any>[] = [
      this._api.getProducts(0),
      this._api.getCatalogs()
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data: any) => {
        this.products = data[0];
        this.catalogs = data[1];
        this.catalogs.forEach(x => {
          this.showShieldBack.set(x.catalogId, false);
        })
      });
  }

  addProduct() {
    if (this.dropDown != null && this.dropDown.length > 0)
      this.dropDown = [];
    var catal = new ModalModelList();
    catal.dropId = "drpCatalog";
    catal.id = "catalogId";
    catal.label = "Catalogo";
    catal.list.push(this.catalogs);
    catal.valueDisplay = "catalogName";
    this.dropDown.push(catal);

    this.visibility = !this.visibility;
   
  }

  onModalConfirmed(event:Map<string, string>) {
    this.visibility = false;
    
    var newProduct: Product = { productName: event.get("productName"), productPrice: Number.parseFloat(event.get("productPrice") || "0"), catalogId: Number.parseInt(event.get("catalogId") || "0") };

    this._api.postProduct(newProduct).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(() => {
        this.visibility = false;
        this.getProducts(null);
      });
  }

  onCatalogChange(event: any) {
    this.getProducts(event);
  }

  deleteProduct(event: any) {
    var productId = event.target.id;
    this._api.deleteProduct(productId).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(() => {
        this.getProducts(null);
      })
  }

  getProducts(event: any) {
    this._api.getProducts(event != null ? event.target.value: 0).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data: any) => {
        this.products = data;
      });
  }

  onCatalogClick(catalogId: any) {
    this.showShieldBack.forEach((v, k) => {
      this.showShieldBack.set(k, false);
    })
    
    this.showShieldBack.set(catalogId, !this.showShieldBack.get(catalogId));

    this.productsFiltered = this.products.filter(x => x.catalogId == catalogId);
      
  }

  addProductToCatalog(catalogId: any) {
    this.modalViews = [{
      type: ModalModelEnum.TextBox, viewItems: [
        { viewId: "txtProductName", referenceId: "productName", label: "Nome bottiglia" },
        { viewId: "txtProductPrice", referenceId: "productPrice", label: "Prezzo" }
      ]
    }];
    this.selectedCatalog = catalogId;
    this._modal.showAddModal(this.onProductAdded, "Nuovo articolo", this.modalViews, this.modalType);

  }

  onProductAdded = (info: any): void => {
    var product: Product = {
      productName: info.get("productName"),
      productPrice: info.get("productPrice"),
      catalogId: this.selectedCatalog
    }

    this._api.postProduct(product).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe(() => {
        this.selectedCatalog = 0;
        this.initData();
      })
  }

  addNewCatalog() {
    this.modalViews = [
      { type: ModalModelEnum.TextBox, viewItems: [{ viewId: "txtCatalogName", referenceId: "catalogName", label: "Nome catalogo" }] }
    ];

    this._modal.showAddModal(this.onCatalogAdded, "Nuovo catalogo", this.modalViews, this.modalType);
  }

  onCatalogAdded = (info: any): void => {
    var catalog: Catalog = {
      catalogName: info.get("catalogName")
    }

    this._api.postCatalog(catalog).pipe(
      catchError(err => {
        this._modal.showErrorModal(err.message);
        return err;
      })).subscribe(() => {
        this.initData();
      })
  }

}
