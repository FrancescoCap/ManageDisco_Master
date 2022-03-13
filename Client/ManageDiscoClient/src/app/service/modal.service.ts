import { ComponentFactoryResolver, ComponentRef, Input, ViewChild, ViewContainerRef } from "@angular/core";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ModalComponent } from "../components/modal/modal.component";
import { ModalViewGroup } from "../components/modal/modal.model";
import { ModalType } from "../model/models";

@Injectable()
export class ModalService {

  @Input() modalContainer?: ViewContainerRef;
  modalComponentRef?: ComponentRef<ModalComponent>;
  componentFactory?: any;
 
  constructor(private factory: ComponentFactoryResolver) {
    
  }

  public setContainer(container: ViewContainerRef) {
    this.modalContainer = container;
  }

  public setModelView(ngModel: Map<any, any>) {   
    if (this.modalComponentRef != null)
        this.modalComponentRef.instance.modelValues = ngModel;
  }

  public showErrorOrMessageModal(message: any, title?: any, isMessageModal?:boolean) {
    this.initComponentRef();
    
    if (this.modalComponentRef != null) {
      this.modalComponentRef.instance.modalType = isMessageModal != null && isMessageModal ? ModalType.INFO : ModalType.ERROR;
      this.modalComponentRef.instance.message = message;
      this.modalComponentRef.instance.header = title != null ? title : "Errore";
      this.modalComponentRef.instance.visibile = true;
    }

  }

  public showOperationResponseModal(message: string, title: string, isErrorOperation?: boolean, views?: ModalViewGroup[], onCloseModal?: (info: any) => any) {
    this.initComponentRef();
    if (this.modalComponentRef != null) {
      this.modalComponentRef.instance.modalType = !isErrorOperation ? ModalType.SUCCESS : ModalType.ERROR;

      if (views != null)
        this.modalComponentRef.instance.lists = views;

      this.modalComponentRef.instance.header = title;
      this.modalComponentRef.instance.message = message;
      this.modalComponentRef.instance.modalClose.subscribe(event => {
        if (onCloseModal != null)
          onCloseModal(event);
      })
       
      this.modalComponentRef.instance.visibile = true;
    }
  }

  public showAddModal<T>(confirmCallback: (...info: any) => any, title: string, inputs: ModalViewGroup[], modalType?:ModalType) {
    this.initComponentRef();
    
    if (this.modalComponentRef != null) {
      
      for (var i = 0; i < inputs.length; i++) {        
        this.modalComponentRef.instance.lists.push(inputs[i]);
        inputs[i].viewItems.forEach((x, y) => {
          if (x.defaultText != null) {
            this.modalComponentRef?.instance.values.set(x.referenceId, x.defaultText);
          }
            
        });
      }

      this.modalComponentRef.instance.modalType = modalType != null ? modalType : ModalType.NEW_RESERVATION;  //default selection for map purposes
      this.modalComponentRef.instance.visibile = true;
      this.modalComponentRef.instance.header = title;
      this.modalComponentRef.instance.modalConfirmed.subscribe((event: any) => {
        
        confirmCallback(event);
      })
    }
  }

  public showListViewModal(title:string, data:ModalViewGroup[]) {
    this.initComponentRef();

    if (this.modalComponentRef != null) {

      this.modalComponentRef.instance.modalType = ModalType.LISTVIEW;
      this.modalComponentRef.instance.lists = data;
      this.modalComponentRef.instance.header = title;
      this.modalComponentRef.instance.visibile = true;
    }
  }

  initComponentRef() {
    var componentFactory = this.factory.resolveComponentFactory(ModalComponent);

    if (this.modalContainer != null) {
      this.modalComponentRef = this.modalContainer.createComponent(componentFactory);
    }

  }

}
