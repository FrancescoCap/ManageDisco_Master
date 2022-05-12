import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { NgModel } from "@angular/forms";
import { Subject } from "rxjs";
import { ModalType, NewTableOrderData } from "../../model/models";
import { ModalService } from "../../service/modal.service";

import { ModalModelEnum, ModalViewGroup, ViewItem } from "./modal.model";

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
  //To close manually modal
  modalVisibilityListener?: Subject<boolean>;
  @Input("visibility") visibile: boolean = false;
  @Input("title") header: string = "";
  @Input("modalType") modalType?: ModalType;

  modelValues?: Map<any,any>;
  productList?: any;
  modelOut?: any;

  disableQuantity = true;

  disableViewQuantityMap: Map<number, boolean> = new Map<number, boolean>();
  viewQuantityValuesMap: Map<number, string> = new Map<number, string>();

  constructor(private _modalService: ModalService) { }

  ngOnInit(): void {
    this.initializeRetValues();
    this.initTabletxtQuantityFlags();
    
  }

  showModal() {
    this.modalVisibilityListener = new Subject<boolean>();
    this.modalVisibilityListener?.subscribe((val: boolean) => { this.visibile = val; })
  }
  
  initTabletxtQuantityFlags() {
    
    this.lists.forEach((x, y) => {
      x.viewItems.forEach((k: ViewItem, i: number) => {
        if (x.txtTableQuantity) {
          k.list?.forEach((el: any, elIndex: number) => {
            this.disableViewQuantityMap?.set(elIndex, true);
            this.viewQuantityValuesMap?.set(elIndex, "");
          })
        }
      })
    })
  }

  /**
   * Dato il nr. di riga recupero i valori di attivazione e il valore testuale dalle array inizializzate nell'ngInit
   * @param rowIndex 
   * @param event checkbox event
   * @param hasTextQuantity true if need to handle quantity for row checked
   * @param object, object of list data supplied to table
   */
  onCheckboxChangeState(rowIndex: number, referenceId:any, event: any, hasTextQuantity?: boolean, object?:any, connectToReferenceView?:string) {
    var isChecked = event.target.checked;
    
    if (isChecked) {
      var keyValueList = this.values.get(referenceId);
      
      if (hasTextQuantity) {
        this.disableViewQuantityMap!.set(rowIndex, !event.target.checked);
        this.viewQuantityValuesMap!.set(rowIndex, "1");
        this.modelOut = this.modelOut == null ? object._valueOut : this.modelOut + object._valueOut;               
        
        var valueExist = keyValueList?.includes(object._dropId, 0);

        if (valueExist) {
          var valueIndex = keyValueList?.findIndex(x => x == referenceId._dropId, 0);
          keyValueList!.splice(valueIndex!, 1);
        } else {
          if (keyValueList == null) {
            this.values.set(referenceId, [object._dropId + ":1"])
          } else
            this.values.get(referenceId)?.push(object._dropId + ":1");
        }
        if (connectToReferenceView != null)
          this.values.set(connectToReferenceView, this.modelOut);
      }
    } else if (!isChecked && hasTextQuantity) {
      this.viewQuantityValuesMap!.set(rowIndex, "");
      this.modelOut = this.modelOut - object._valueOut;
      this.disableViewQuantityMap!.set(rowIndex, true);
      var valueIndex = this.values.get(referenceId)?.findIndex(x => x._dopId == object);
      this.values.get(referenceId)?.splice(valueIndex!, 1);
    }
  }
  
  onTxtTableQuantityChange(rowIndex: number, referenceId: any, event: any, object?: any) {
    var valueInput = event.target.value;
    var oldValueInput = +this.viewQuantityValuesMap.get(rowIndex)!;
    this.viewQuantityValuesMap.set(rowIndex, valueInput);
    //+ sign is to make a try parse
    if (+valueInput != NaN) {
      this.modelOut += (valueInput * object._valueOut) - (oldValueInput * object._valueOut);
      var objectIndex = this.values.get(referenceId)?.findIndex(x => x.split(":")[0] == object._dropId);
      this.values.get(referenceId)![objectIndex!] = object._dropId + ":" + valueInput;
    }
  }

  onChange(event: any, referenceId?: any) {
    var value = event.target.value;
    this.values.set(referenceId, value);
  }


  dropDownValueChange(e: any) {
    this.selectedValueChange.emit(e);
  }

  onTxtChange(referenceId: any, value:any) {
    this.values.set(referenceId, value)
  }

  onConfirmModal() {
    switch (this.modalType) {
      case ModalType.NEW_RESERVATION:
      case ModalType.NEW_PRODUCT:
        this.modalConfirmed.emit(this.values);
        break;
      case ModalType.LOGIN:
        this.modalConfirmed.emit(this.values);
        break; 
    }
    //this.visibile = false;
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
 
  //onCheckboxChangeState(keyRef: any, product: any, event:any, tableSelector:any) {
    
  //  this.removeChecked(tableSelector, keyRef);

  //  if (this.values.get(keyRef) == null) {
  //    this.values.set(keyRef, [product._dropId]);    
  //  } else {
  //    //get list
  //    var keyValueList = this.values.get(keyRef);

  //    var valueExist = keyValueList?.includes(product._dropId,0);
      
  //    if (valueExist) {
  //      var valueIndex = keyValueList?.findIndex(x => x == product._dropId, 0);
  //      keyValueList!.splice(valueIndex!, 1);
  //    } else {
  //      keyValueList!.push(product._dropId);       
  //    }      
      
  //    this.values.set(keyRef, keyValueList!);     
  //  }
  //  /*
  //   * Se _valueOut della lista è diverso da null significa che c'è un valore che deve essere letto da un'altra view.
  //   * caso TRUE: valorizzo il modelout che andrà a leggere la view. La view con l'ngmodel abilitato ha il flag HasNgModel = true.
  //   * case FALSE: non faccio nulla
  //   *
  //   * IL TUTTO E' DA MIGLIORARE PERCHE' NON E' DA ESCLUDERE CHE QUESTA LOGICA FUNZIONI SOLO PER TABELLA - TEXTBOX E CON UNA RELAZIONE 1 A 1
  //   * VISIONARE ANCHE L'HTML
  //   */
  //  if (product._valueOut != null) {
  //    var addValue = event == null ? true : event.target.checked;
      
  //    this.modelOut = this.modelOut == null ? product._valueOut : (addValue ? this.modelOut + product._valueOut : this.modelOut - product._valueOut);
  //    var ngModelReference = this.modelValues?.get(keyRef);
  //    this.values.set(ngModelReference, this.modelOut);
      
  //  }

  //  if (this.lists.find(x => x.txtTableQuantity == true)) {
  //    console.log(this.disableViewMap)
  //    console.log(keyRef)
  //    this.disableViewMap?.set(keyRef, !event.target.checked)
  //  }
  //}

  //onTxtTableQuantityChange(keyRef: any, event: any, productId: any) {
  //  var quantity = event.target.value;

  //  if (this.values.get(keyRef) == null) {
  //    this.values.set(keyRef, [productId + ":" + quantity]);
  //  } else {
  //    //get list
  //    var keyValueList = this.values.get(keyRef);

  //    var valueExist = keyValueList?.find(x => x.split(":")[0] == productId);      

  //    if (valueExist) {
  //      var valueIndex = keyValueList?.findIndex(x => x.split(":")[0] == productId, 0);
  //      keyValueList!.splice(valueIndex!, 1);
  //    }

  //    if (quantity > 0) {
  //      keyValueList!.push(productId + ":" + quantity);
  //    }        
     
  //    this.values.set(keyRef, keyValueList!);
  //  }
  //}

  onSyncTextBoxInputChange(view: ViewItem, event: any) {
    view.onSyncTextBoxInputChange!(event);
  }

  removeChecked(tableSelector:any, keyRef:any) {
    var table = this.lists.find(x => x.selector == tableSelector);
   
    if (!table?.multiSelect) {      
      table?.viewItems[0].list?.forEach((x, y) => {
        this.values.set(keyRef, []);
      });      
    }
  }

 
  onFileChanged(fileEvent: any, referenceId:any) {
    
    let fileReader = new FileReader();

    fileReader.onloadend = (e: any) => {
      this.values.set(referenceId, e.target.result);
    }
    fileReader.readAsDataURL(fileEvent.target.files[0]);
   
  }

  inputChange(viewItem: ViewItem, event: any) {
    setTimeout(function () {
      if (viewItem.validationFunc != null) {
        //Call callback only if coupon code are completed. It is not suitable for other validation
        if (event.target.value.length == 6) {
          viewItem.validationFunc(event.target.value);
          viewItem.extraDescription?.subscribe((value: any) => { viewItem.extraDescriptionString = value });
        }        
      }
    },500)   
  }
}
