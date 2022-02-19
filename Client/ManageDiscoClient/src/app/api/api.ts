import { HttpErrorResponse, HttpStatusCode } from "@angular/common/http";
import { Injectable, Input, ViewContainerRef } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, map, Observable, pipe } from "rxjs";
import { ModalComponent } from "../components/modal/modal.component";
import { GeneralMethods } from "../helper/general";
import { AssignTablePost, Catalog, EventParty, PaymentOverview, PaymentPost, Product, Reservation, ReservationType, Table, TableMapFileInfo, TableEvents, TableOrderPut, TableOrderHeader, EventPartyView, PrCustomerView, HomeInfo, TableOrderRow, CatalogView, HomePhotoPost } from "../model/models";
import { ModalService } from "../service/modal.service";

import { ApiHttpService } from "./http";
import { Endpoints } from "./url";

@Injectable()
export class ApiCaller {

  modalContainer?: ViewContainerRef;

  constructor(
    private url: Endpoints,
    private http: ApiHttpService,
    private modalService: ModalService,
    private router: Router) {
    
  }

  public setModalContainer(container:ViewContainerRef) {
    this.modalContainer = container;
    this.modalService.setContainer(container);
  }

  onApiError = (status: number):void => {
    if (status == 0) {  //Unable to connect to server
      this.modalService.showErrorOrMessageModal("Impossibile raggiungere il server.");
    }else if (status == HttpStatusCode.Unauthorized) {
      this.router.navigateByUrl("/Login");     
    }
  }

  public logout() {
    return this.http.postCall(this.url.logout(), null);
  }

  public postContact(contact: any): Observable<any>{
    return this.http.postCall(this.url.postContact(), contact);
  }

  public getContact() {
    return this.http.getCall(this.url.getContact());
  }

  public getContactTypes() {
    return this.http.getCall(this.url.getContactTypes());
  }

  public putWarehouseQuantity(warehouse:any) {
    return this.http.putCall(this.url.putWarehouseQuantity(), warehouse);
  }

  public getWarehouse(): Observable<any> {
    return this.http.getCall(this.url.getWarehouse());
  }

  public postHomePhoto(data:HomePhotoPost[]) {
    return this.http.postCall(this.url.getHome(), data);
  }

  public deleteEvent(eventId:any):Observable<any> {
    return this.http.deleteCall(this.url.getEvents(eventId));
  }

  public postCatalog(catalogData:any): Observable<any> {
    return this.http.postCall(this.url.getCatalog(), catalogData);
  }

  public putChangePrCustomer(prCode:any) {
    return this.http.putCall(this.url.getPrCustomers(), prCode);
  }

  public getHomeInfo():Observable<any> {
    return this.http.getCall(this.url.getHomeInfo()).pipe(
      map((data:HomeInfo) => {
        data.events!.forEach((x, y) => {
          x.linkImage?.forEach((p, i) => {            
            p = GeneralMethods.normalizeBase64(p);
            x.linkImage![i] = p;
          })
          
        })
        data.homePhoto!.forEach((x, y) => {
          x.base64Image?.forEach((p, i) => {
            p = GeneralMethods.normalizeBase64(p);
            x.base64Image![i] = p;
          })
        })
        return data;
      }));
  }

  public putUserInfo(userInfo:any):Observable<any> {
    return this.http.putCall(this.url.putUserInfo(), userInfo);
  }

  public getUserReservation() {
    return this.http.getCall(this.url.getUserReservation(), this.onApiError);
  }

  public getUserInfo(): Observable<any> {
    return this.http.getCall(this.url.getUserInfo(), this.onApiError);
  }

  public getMenu(): Observable<any> {
    return this.http.getCall(this.url.getMenu());
  }


  public register(data: any):Observable<any> {
    return this.http.postCall(this.url.register(), data);
  }

  public login(data: any): Observable<any> {
    return this.http.postCallWithResponse(this.url.login(), data);
  }
  //eventId optional for filtering purposes
  public getReservations(eventId: number, reserveStatus?: number): Observable<Reservation[]> {
    return this.http.getCall(this.url.getReservations(eventId, reserveStatus));
  }


  public getReservationStatus() {
    return this.http.getCall(this.url.getReservationStatus());
  }

  //eventId optional for filtering purposes
  //public getAcceptedReservations(eventId: number): Observable<Reservation[]> {
  //  return this.http.getCall(this.url.getReservations(eventId, )).pipe(
  //    map((data:Reservation[]) => {
  //      data.map((item: Reservation) => {
  //        return item;
  //      })
  //      return data;
  //    }));
  //}

  public getEvents():Observable<EventPartyView> {
    return this.http.getCall(this.url.getEvents(), this.onApiError).pipe(
      map((data: EventPartyView) => {
        data.events!.map((item: EventParty) => {
          item._showDate = new Date(item.date!).getFullYear() > 2020;
          item._dropId = item.id;
          item._modalDropText = item.name;
          //item.description = item.description?.replace('\n', '<br/>');
          item.imagePreview = GeneralMethods.normalizeBase64(item.imagePreview);
          return item;
        })        
        return data;
      }));
  }

  public getEventDetail(id: number) {
    return this.http.getCall(this.url.getEventDetail(id)).pipe(
      map((data:EventParty) => {
        data.linkImage?.forEach((x, y) => {
          x = GeneralMethods.normalizeBase64(x);
          data.linkImage![y] = x;
        })
        return data;
      }));
  }

  public putCancelEvent(eventId: number) {   
    return this.http.putCall(this.url.putCancelEvent(eventId), null);
  }

  public editEventPrices(eventId:any, data:any) {
    return this.http.putCall(this.url.editEventDetails(eventId), data, this.onApiError);
  }

  public postEvent(event:EventParty):Observable<any> {
   return this.http.postCall(this.url.getEvents(), event);
  }

  public getReservationTypes(): Observable<ReservationType[]> {
    return this.http.getCall(this.url.getReservationTpe()).pipe(
      map(data => {
        data.map((item: ReservationType) => {
          item._dropId = item.reservationTypeId;
          item._modalDropText = item.reservationTypeString;
          return item;
        })
        return data;
      }));
  }
  
  public postReservation(data?: Reservation) {
    return this.http.postCall(this.url.getReservations(0), data);
  }

  public getPrCustomers(): Observable<PrCustomerView[]> {
    return this.http.getCall(this.url.getPrCustomers()).pipe(
      map((data:PrCustomerView[]) => {
        data.map((item: PrCustomerView) => {
          item._dropId = item.customerId;
          item._modalDropText = item.name + " - " + item.surname;
          return item;
        })
        return data;
      }));
  }

  getReservationEvent(eventId:any): Observable<Reservation> {
    return this.http.getCall(this.url.getEventReservation(eventId));
  }

  public acceptReservation(reservationId: number, status: number) {
    return this.http.putCall(this.url.getReservations(0) + "?reservationId="+ reservationId.toString() + "&status=" + status.toString(), null)
  }

  public confirmReservationBudget(reservationId:number, euro: number): Observable<any> {
   return this.http.putCall(this.url.confirmBudget() + `?reservationId=${reservationId}&euro=${euro}`, null);
  }

  public getPaymentsOverview(): Observable<PaymentOverview[]> {
    return this.http.getCall(this.url.paymentOverview()).pipe(
      map((data:PaymentOverview[]) => {
        data.map((item:PaymentOverview) => {
          item.userIdView = "btnDetails_" + item.userId;
          return item;
        })
        return data;
      })
    );
  }

  public getPaymentsOverviewDetails(userId:string) {
    return this.http.getCall(this.url.paymentOverviewDetails(userId));
  }

  public postPayment(newPayment: PaymentPost) {
    return this.http.postCall(this.url.reservationsPayments(), newPayment);
  }

  public getAssignableTables(eventId?:number): Observable<Table[]> {
    return this.http.getCall(this.url.getAssignableTables(eventId));
  }

  public assignTable(data:AssignTablePost) {
    return this.http.postCall(this.url.postAssignTable(), data);
  }

  public getEventTables(eventId:number):Observable<TableEvents> {
    return this.http.getCall(this.url.getEventTables(eventId));
  }

  public getTableMap<T>(): Observable<TableMapFileInfo> {
    return this.http.getCall(this.url.getTableMap());
  }

  public getCatalogs(): Observable<CatalogView> {
    return this.http.getCall(this.url.getCatalog()).pipe(
      map((data:CatalogView) => {
        data.catalog?.forEach((item: Catalog) => {
          item._modalDropText = item.catalogName;
          item._dropId = item.catalogId;
        })
        return data;
      }));
  }

  public getProducts(catalogId?:number): Observable<Product[]> {
    return this.http.getCall(this.url.getProducts(catalogId)).pipe(
      map(data => {
        data.map((item: Product) => {
          item._dropId = item.productId;
          item._modalDropText = item.productName;
          item._valueOut = item.productPrice;
        });
        return data;
      })
    );
  }

  public postProduct(data:Product) {
    return this.http.postCall(this.url.getProducts(), data);
  }

  public deleteProduct(productId:number) {
    return this.http.deleteCall(this.url.deleteProduct(productId));
  }
  //In realtà più che una put sarebbe una post perchè crea la testata d'ordine associata al tavolo con le rispettive righe
  public putTablOrder(tableId:number, data:TableOrderPut) {
    return this.http.putCall(this.url.putTableOrder(tableId), data);
  }

  public getTableOrderRows(tableId: number):Observable<TableOrderHeader> {
    return this.http.getCall(this.url.getTableOrderRow(tableId)).pipe(
      map((data: TableOrderHeader) => {
        data.rows!.forEach((x, y) => {
          x._modalDropText = x.productName + ` (${x.tableOrderRowQuantity}) `;          
        })
        return data;
      }));
  }
}
