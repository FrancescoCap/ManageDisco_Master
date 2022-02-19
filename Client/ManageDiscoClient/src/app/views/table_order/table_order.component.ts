import { AfterViewInit, ComponentRef, Output, ViewChild } from '@angular/core';
import { EventEmitter } from '@angular/core';
import { Component, ComponentFactoryResolver, OnInit, Type, ViewContainerRef } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { observable } from 'rxjs';
import { catchError, forkJoin, Observable } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { ModalComponent } from '../../components/modal/modal.component';
import { ModalModelEnum, ModalModelList, ModalViewGroup } from '../../components/modal/modal.model';
import { EventParty, EventPartyView, ModalType, NewTableOrderData, Product, TableEventHeader, TableEvents, TableOrderHeader, TableOrderPut, TableOrderRow } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-table-order',
  templateUrl: './table_order.component.html',
  styleUrls: ['./table_order.component.scss', '../../app.component.scss']
})
export class TableOrderComponent implements AfterViewInit {

  @ViewChild('addOrderModal', { read: ViewContainerRef, static: false }) modalTemplate?: ViewContainerRef;


  eventTables: TableEventHeader = { tables: [] };
  eventTablesFull: any[] = []; //Contiene tutta la lista dei tavoli: mi serve per poter ripristinare la lista originale/eseguire un nuovo filtro
  events?: EventPartyView;
  products?: Product[]; //bottiglie
  tableOrderRows?: TableOrderHeader;  //Per ricarico in modal ed eventuale modifica. La carico quando viene cliccato l'edit

  modalNewOrderViews: ModalViewGroup[] = [];
  modalNewOrderNgModel: Map<any, any> = new Map();
  modalAddOrderType = ModalType.NEW_TABLEORDER;
  productsList?: Product[];
  tblId?: any; //id del tavolo per cui fare/modificare un ordine

  selectedEvent: number = 0;
  tableNameFilter: string = "";

  tableIdNewOrder: number = 0;
  addNewOrder = false;
  newOrderModalTitle = "Nuovo ordine per ";

  constructor(private _api: ApiCaller,
    private _componentFactory: ComponentFactoryResolver,
    private _modalService: ModalService) {
  }

  ngAfterViewInit(): void {
    //passo l'ng-container al service che a sua volta lo passa al modalcomponent --> per lo show della modal
    if (this.modalTemplate != null)
      this._modalService.setContainer(this.modalTemplate);
  }

  ngOnInit(): void {        
    this.initData();    
  }

  initData() {
    this.getEventTables(this.selectedEvent);
    this.getEvents();
  }

  getEvents() {
    this._api.getEvents().pipe(catchError(err => {
      console.log(err);
      return err;
    })).subscribe((data: any) => {
      this.events = data;
    })
  }

  //Restituisce i tavoli confermati e sistemati sulla mappa per l'evento richiesto
  getEventTables(eventId: number) {
    this._api.getEventTables(eventId).pipe(catchError(err => {
      console.log(err);
      return err;
    })).subscribe((data: any) => {     
      this.eventTables = data;
      this.eventTablesFull = this.eventTables.tables;
      this.selectedEvent = data.eventId;
    })
  }

  onEventChange() {
    this.getEventTables(this.selectedEvent);
  }

  onTableNameFilter(event?: any) {
    //Faccio il filtro lato client perchè ho bisogno che i dati siano pronti subito e non dover essere soggetto ai tempi del server o della chiamata in generale
    //(che potrebbe essere rallentata per una scarsa connettività)
    /******* FUNZIONA SOLAMENTE SE EVENTTABLESFULL E' NON è DELLO STESSO TIPO DI EVENTTABLES. INFATTI HO DOVUTA METTERLA DI TIPO ANY *******/
    if (event != "") {
      this.eventTables.tables = this.eventTablesFull.filter(x => x.tableName?.toLocaleLowerCase().includes(this.tableNameFilter.toLocaleLowerCase()));
    }
   else {
      this.eventTables.tables = this.eventTablesFull;
    }     
  }

  //Recupera i prodotti e apre la modal per l'inserimento dell'ordine
  addOrderTable(tableName: any, tableId: any) {
    if (this.productsList == null)
      this.initDataForOrder(true, tableId);
    else {
      this.openNewOrderModal(tableId);
    }
    this.tblId = tableId;
  }

  onOrderAdded = (info: any): void => {
    var order: TableOrderPut = {
      exitChanged: info.get("orderExit"),
      productsSpendingAmount: info.get("orderBudget"),
      productsId: {}
    };
    var rows = info.get("productsId");
    if (rows.length == 0)
      return;

    for (var i = 0; i < rows.length; i++) {     
        order.productsId![rows[i]] = 1;
    }
    this._api.putTablOrder(this.tblId, order).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe(x => {
        this.initData();
      });
    
  }

  initNewOrderModalViews() {
    if (this.modalNewOrderViews.length > 0) {
      this.modalNewOrderViews = [];
    }

    this.modalNewOrderViews.push(
      {
        type: ModalModelEnum.Table, viewItems: [
          { viewId: "tblProducts", referenceId: "productsId", list: this.productsList, label: "Prodotti" }
        ]        
      },
      {
        type: ModalModelEnum.TextBox, viewItems: [
          { viewId: "totBudget", referenceId: "orderBudget", label: "Totale", hasNgModel:true },
          { viewId: "txtExit", referenceId: "orderExit", label: "Exit", hasNgModel: false }
        ]
      }      
    )
    /*
     * Per associare i due valori imposto un map nel modal component indicando il reference id del valore da usare come ngModel e a chi assegnarlo
     * @param1 : referenceId dei dati con all'interno il valueOut
     * @param2: referenceId della view che deve leggere quel valore
     * */
    this.modalNewOrderNgModel.set("productsId","orderBudget");
  }

  onOrderConfirmed(orderInfo: NewTableOrderData) {
    var orderData = this.generateOrder(orderInfo);

    this._api.putTablOrder(this.tableIdNewOrder, orderData).pipe(
      catchError(err => {
        console.log(err);
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe(() => {
        this.getEventTables(this.selectedEvent);
      });
  }

  onModalClose(state: any) {
  }

  generateOrder(products: NewTableOrderData): TableOrderPut {

    var orderData: TableOrderPut = {};
    orderData.productsId = products.productsId;
    orderData.productsSpendingAmount = products.totSpending;
    orderData.exitChanged = products.totExit;

    return orderData;
  }

  editOrderTable(tableName: any, tableId: any) {
    const calls: Observable<any>[] = [
      this._api.getTableOrderRows(tableId),
      this.getProducts()
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data: any) => {
        /*DATA VALUES*/
        this.tableOrderRows = data[0];
        this.modalAddOrderType = ModalType.NEW_TABLEORDER;
        this.tableIdNewOrder = tableId;
        /*MODAL VALUES*/
        this.newOrderModalTitle = "Modifica ordine per " + tableName;
      })
  }

  //Faccio ritornare un Observable così posso fare la chiamata runtime in diversi punti
  public getProducts(): Observable<Product[]> {
    return this._api.getProducts();
  }

  //Map inteso come tipo di oggetto
  getProductsQtyMap(tableOrderRows?: TableOrderRow[]): { [key: number]: number } {
    var map: { [key: number]: number } = {};

    tableOrderRows?.map(x => {
      if (x.productId != null)
        map[x.productId] = x.tableOrderRowQuantity || 0;
    });

    return map;
  }
  /**************************
   * Recupero tutti i dati che mi servono per gli ordini:
   * 1. I prodotti per entrambi i tipi di inserimento: aggiunta / modifica
   * 2. I dati dell'ordine in caso di modifica
   *************************/
  initDataForOrder(addOrder: boolean, tableId?: any) {
    const observables: Observable<any>[] = [
      this._api.getProducts()
    ]
    if (!addOrder) {
      observables.push(this._api.getTableOrderRows(tableId))
    }

    forkJoin(observables).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.productsList = data[0];
        if (!addOrder) {
          this.tableOrderRows = data[1];
        }
        this.openNewOrderModal(tableId);
      });
  }

  openNewOrderModal(tableId:any) {
    //apro la modal
    this.initNewOrderModalViews();
    const tableName = this.eventTables.tables.find(x => x.tableId == tableId)?.tableName;
    this._modalService.showAddModal(this.onOrderAdded, `Nuovo ordine per ${tableName}`, this.modalNewOrderViews);
    this._modalService.setModelView(this.modalNewOrderNgModel);
  }

  getOrderHistory(tableId: any) {
    this._api.getTableOrderRows(tableId).pipe(
      catchError(err => {
        this._modalService.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.openOrderHistoryModal(data.rows, tableId);
      })
  }

  openOrderHistoryModal(orderHistory: any[], tableId:any) {    
    var modalViews: ModalViewGroup[] = [{
      type: ModalModelEnum.Dropdown, viewItems: [{
        list: orderHistory
      }]
    }];
    const tableName = this.eventTables.tables.find(x => x.tableId == tableId)?.tableName;
    this._modalService.showListViewModal("Storico ordini per " + tableName, modalViews);
  }
}

