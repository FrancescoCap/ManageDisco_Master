import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { NgModel } from "@angular/forms";
import { ModalType, NewTableOrderData } from "../../model/models";
import { ModalService } from "../../service/modal.service";

import { ModalImageBoxList, ModalModelEnum, ModalModelList, ModalTextBoxList, ModalViewGroup } from "./modal.model";

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.style.css']
})
export class ModalComponent implements OnInit {

  six: string  = ""

  productsList_Original?: any[];
  message = "";

  /******* OUTPUT EVENT **********/
  @Output("onDropChange") selectedValueChange: EventEmitter<any> = new EventEmitter<any>();
  @Output("onModalConfirmed") modalConfirmed: EventEmitter<any> = new EventEmitter<any>();
  @Output("onModalClose") modalClose: EventEmitter<any> = new EventEmitter<any>();
  @Output("onOrderConfirmed") orderConfirmed: EventEmitter<any> = new EventEmitter<any>();

  reservationVals: string[] = ["", "", "", ""];
  values: Map<any, any[]> = new Map<any, any[]>();

  /********DATA OUTPUTONLY********/
  productSelected: NewTableOrderData = { productsId: {} };

  /********DATA INPUT*********/
  @Input("lists") lists: ModalViewGroup[] = [];

  @Input("textBox") textBox?: ModalViewGroup = { type: ModalModelEnum.TextBox, viewItems: [] };
  @Input("imgBox") imgBox?: ModalViewGroup = { type: ModalModelEnum.ImageBox, viewItems: [] };
  @Input("imgBox") table?: ModalViewGroup = { type: ModalModelEnum.Table, viewItems: [] };

  @Input("inputModel") input?: any;
  @Input("visibility") visibile: boolean = false;
  @Input("title") header: string = "";
  @Input("modalType") modalType?: ModalType;

  modelValues?: Map<any,any>;
  productList?: any;
  modelOut?: any;

  constructor(private _modalService: ModalService) { }

  ngOnInit(): void {  
    this.initializeRetValues();
  }

  dropDownValueChange(e: any) {
    this.selectedValueChange.emit(e);
  }

  onConfirmModal() {
    //var output;
    switch (this.modalType) {
      case ModalType.NEW_RESERVATION:
      case ModalType.NEW_PRODUCT:
        if (this.modalType == ModalType.NEW_PRODUCT) {
          this.setDataForOrder();
        }
        this.modalConfirmed.emit(this.values);
        break;
      case ModalType.NEW_PAYMENT:
        this.modalConfirmed.emit(this.input);
        break;
      case ModalType.NEW_TABLEORDER:
        this.setDataForOrder();
        this.orderConfirmed.emit(this.productSelected);
        break;
    }
    this.visibile = false;
  }
  
  onCloseModal() {
    this.visibile = false;
    this.modalClose.emit(this.visibile);
  }

  //inizializzo la lista che dovrà essere restituita al verificarsi dell'evento onModalConfirmed(). La lista stessa viene restituita (per ora) solo quando modalType = NEW_RESERVATION/PRODUCT
  initializeRetValues(): number {
    var controlsCount = 0;
    if (this.lists != null) {
      //this.lists.viewItems.forEach(x => {
      //  this.values.set(x.referenceId, "");
      //})
      //controlsCount += this.lists.viewItems.length;
    }

    if (this.textBox != null) {
      this.textBox.viewItems.forEach(x => {
        this.values.set(x.referenceId, [{}]);
        //this.modelValues.push({});
      })
      controlsCount += this.textBox.viewItems.length;
    }

    if (this.table != null) {
      this.table.viewItems.forEach(x => {
        this.values.set(x.referenceId, [{}]);
        //this.modelValues.push({});
      });
      controlsCount += this.table.viewItems.length;
    }
    
    return controlsCount;
  }

  onCheckboxChangeState(keyRef: any, product: any) {
   
    if (this.values.get(keyRef) == null) {
      this.values.set(keyRef, [product._dropId]);    
    } else {
      //get list
      var keyValueList = this.values.get(keyRef);

      var valueExist = keyValueList?.includes(product._dropId,0);
    
      if (valueExist) {
        var valueIndex = keyValueList?.findIndex(x => x == product._dropId, 0);
        keyValueList!.splice(valueIndex!, 1);
      } else {
        keyValueList!.push(product._dropId);       
      }      
      
      this.values.set(keyRef, keyValueList!);      
    }
    /*
     * Se _valueOut della lista è diverso da null significa che c'è un valore che deve essere letto da un'altra view.
     * caso TRUE: valorizzo il modelout che andrà a leggere la view. La view con l'ngmodel abilitato ha il flag HasNgModel = true.
     * case FALSE: non faccio nulla
     *
     * IL TUTTO E' DA MIGLIORARE PERCHE' NON E' DA ESCLUDERE CHE QUESTA LOGICA FUNZIONI SOLO PER TABELLA - TEXTBOX E CON UNA RELAZIONE 1 A 1
     * VISIONARE ANCHE L'HTML
     */
    if (product._valueOut != null) {
      this.modelOut = this.modelOut == null ? product._valueOut : this.modelOut + product._valueOut;
      var ngModelReference = this.modelValues?.get(keyRef);
      this.values.set(ngModelReference, this.modelOut);
    }
  }

  onChange(event: any, referenceId?: any) {
    this.values.set(referenceId, event);
  }

  onTxtProductFilterChange(input: any) {
    if (this.lists != null) {
      const filterString = input.data;
    }
  
  }

  editQuantity(productId: any, quantity: any) {
    //if (this.lists.viewItems[0].mapList != null) {
    //  const oldQty = this.lists.viewItems[0].mapList[productId];
    //  this.lists.viewItems[0].mapList[productId] = oldQty + quantity;
    //  this.lists.viewItems[0].viewId = quantity == 1 ?
    //    this.lists.viewItems[0].viewId + this.productList.find((x: any) => x.productId == productId).productPrice :
    //    this.lists.viewItems[0].viewId - this.productList.find((x: any) => x.productId == productId).productPrice
    //}
  }

  productIsSelected(productId: any) {

    //if (this.lists != null && this.lists.viewItems[0] != null && this.lists.viewItems[0].mapList != null)
    //  return this.lists.viewItems[0].mapList[productId] > 0;
    //else
    //  return false;
  }
  /***********************************
   * Imposta i dati dell'ordine prendendo le infomarzioni dal modello generico (lists = ModalModelList)
   * Casistica: Ordine-bottiglie
   ***********************************/
  setDataForOrder() {
    //if (this.lists != null && this.lists.viewItems[0].mapList != null) {
    //  this.lists.viewItems.forEach((k, s) => {
    //    var mapToArray = this.lists.viewItems[s].mapList;
    //    this.productSelected.productsId = mapToArray[s] as Array<any>;
    //    this.productSelected.totExit = this.lists.viewItems[0].referenceId;
    //    this.productSelected.totSpending = this.lists.viewItems[0].viewId;
    //  });
    //}

    if (this.table != null /*&& this.table.viewItems[0].mapList != null*/) {      
      //Fare ciclo un ciclo per rendere la parte di table più scalabile. Non posso in automatico prendere sempre l'index 0 perchè potrei avere più oggetti di tipo table
      console.log(this.table.viewItems)
      this.table.viewItems.forEach((k, l) => {                
       console.log(k.mapList![k.referenceId])
      })
    
      //var key = this.table.viewItems[0].referenceId;
      //this.values.set(key, this.table.viewItems[0].mapList[key]);
      
    }
  }

  onFileChanged(fileEvent: any, referenceId:any) {
    
    let fileReader = new FileReader();

    fileReader.onloadend = (e: any) => {
      this.values.set(referenceId, e.target.result);
    }
    fileReader.readAsDataURL(fileEvent.target.files[0]);
   
  }
}
