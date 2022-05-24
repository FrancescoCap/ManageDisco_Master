import { HttpStatusCode } from "@angular/common/http";
import { Injectable, Input, ViewContainerRef } from "@angular/core";
import { Router } from "@angular/router";
import { map, Observable, pipe } from "rxjs";
import { GeneralMethods } from "../helper/general";
import { AssignTablePost, Catalog, EventParty, PaymentOverview, PaymentPost, Product, Reservation, ReservationType, Table, TableMapFileInfo, TableEvents, TableOrderPut, TableOrderHeader, EventPartiesViewInfo, PrCustomerView, HomeInfo, CatalogView, HomePhotoPost, Role, TranslatedRolesEnum, ReservationStatus, TranslatedReservationStatusEnum, ProductShopHeader, ProductShopType, UserPermissionCell, UserPermissionTable, UserPermissionPut, CouponValidation, ReservationPost, PaymentsOverviewFull, NewCollaboratorInfo, ProductShopView, UserInfoView } from "../model/models";
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

  onApiError = (status: number, message: string): void => {
    if (status == 0) {  //Unable to connect to server      
      this.modalService.showErrorOrMessageModal("Impossibile raggiungere il server.");
    } else if (status == HttpStatusCode.Unauthorized) {     
      if (!this.router.url.includes("Login")) {
        //this.getRefreshToken().subscribe(() => {
        //    console.log(this.router.url)
        //    //this.router.navigateByUrl('http://192.168.1.69:4200/Events');
        //  });
      }     
    } else if (status == HttpStatusCode.BadRequest) {
      this.modalService.showErrorOrMessageModal(message, "ERRORE");
    }
  }

  public getProfilePageViewType() {
    return this.http.getCall(this.url.getProfilePageViewType(), this.onApiError);
  }

  public exportTables(eventId:number) {
    return this.http.getFileCall(this.url.exportTables(eventId), this.onApiError);
  }

  public postAutoAssignTable(eventId?:number) {
    return this.http.postCall(this.url.postAutoAssignTable(eventId), null, this.onApiError);
  }

  public putUserPermission(data: UserPermissionPut) {
    return this.http.putCall(this.url.putUserPermission(), data, this.onApiError);
  }

  public getUserPermission() {
    return this.http.getCall(this.url.getUserPermission(), this.onApiError);
  }

  public getPermissionAction() {
    return this.http.getCall(this.url.getPermissionAction(), this.onApiError);
  }

  private checkTokenValidation() {
    return this.http.getCall(this.url.getEvents(), this.onApiError)
  }

  public getRefreshToken() {
    return this.http.getCall(this.url.getRefreshToken());
  }

  public closeFreeEntrance(eventId:any) {
    return this.http.putCall(this.url.closeFreeEntrance(eventId), this.onApiError)
  }

  public checkCouponValidation(code:string) {
    return this.http.getCall(this.url.checkCouponValidation(code), this.onApiError);
  }

  public getUserAwards() {
    return this.http.getCall(this.url.getUserAwards(), this.onApiError);
  }

  public purchaseProduct(productId:any) {
    return this.http.postCall(this.url.purchaseProduct(productId),null, this.onApiError)
  }

  public postShop(data:ProductShopHeader) {
    return this.http.postCall(this.url.getShop(), data, this.onApiError);
  }

  public getProductShopType() {
    return this.http.getCall(this.url.getProductShopType(), this.onApiError).pipe(
      map(data => {
        data.map((productType: ProductShopType) => {
          productType._dropId = productType.productShopTypeId;
          productType._modalDropText = productType.productShopTypeDescription;
        })        

        return data;
      }));
  }

  public getShop() {
    return this.http.getCall(this.url.getShop(), this.onApiError).pipe(
      map((data: ProductShopView) => {
        data.items?.forEach((o, i) => {
          data.items![i].productShopBase64Image = GeneralMethods.normalizeBase64(o.productShopBase64Image!);
        })
        return data;
      }));
  }

  public getCollaborators() {
    return this.http.getCall(this.url.getCollaborators(), this.onApiError);
  }

  public getAddingCollaboratorInfo() {
    return this.http.getCall(this.url.getAddingCollaboratorInfo(), this.onApiError).pipe(
      map((info: NewCollaboratorInfo) => {
        info.roles?.forEach(x => {
          //index 0 è la chiave dell'enum e 1 il valore
          x._translatedRole = Object.entries(TranslatedRolesEnum).find(k => k[0] == x.name)?.[1];
        });
        return info;
      }));
  }

  public getStatisticsForEvent(eventId:any) {
    return this.http.getCall(this.url.getStatistics(eventId), this.onApiError);
  }

  public confirmUserPhoneNumber(refer:any): Observable<any> {
    return this.http.postCall(this.url.confirmUserPhoneNumber(refer), this.onApiError);
  }

  //API per la ricezione del messaggio whatsapp dell'omaggio
  public getSubscription(eventId?:any) {
    return this.http.postCall(this.url.getSubscription(eventId), this.onApiError);
  }

  //Api per la richiesta dell'omaggio
  public getFreeEntrance(eventId:any): Observable<any> {
    return this.http.getCall(this.url.getFreeEntrance(eventId), this.onApiError);
  }

  public getCoupon(refer:any):Observable<any>{
    return this.http.getCall(this.url.getCoupon(refer));
  }

  public getCouponInfo(refer: any): Observable<any> {
    return this.http.postCall(this.url.getCouponInfo(refer), this.onApiError);
  }

  public logout() {
    return this.http.postCall(this.url.logout(), null);
  }

  public postContact(contact: any): Observable<any>{
    return this.http.postCall(this.url.postContact(), contact);
  }

  public getContact() {
    return this.http.getCall(this.url.getContact(), this.onApiError);
  }

  public getContactTypes() {
    return this.http.getCall(this.url.getContactTypes());
  }

  public putWarehouseQuantity(warehouse:any) {
    return this.http.putCall(this.url.putWarehouseQuantity(), warehouse, this.onApiError);
  }

  public getWarehouse(): Observable<any> {
    return this.http.getCall(this.url.getWarehouse(), this.onApiError);
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

  public putChangePrCustomer(prCode: any) {
    return this.http.putCall(this.url.getPrCustomers(prCode),null, this.onApiError);
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

  public getUserInfoFromId(userId: any) {
    return this.http.getCall(this.url.getUserInfoFromId(userId));
  }

  public getUserInfo(): Observable<UserInfoView> {
    return this.http.getCall(this.url.getUserInfo(), this.onApiError);
  }

  public getMenu(): Observable<any> {
    return this.http.getCall(this.url.getMenu(), this.onApiError);
  }

  public register(data: any):Observable<any> {
    return this.http.postCall(this.url.register(), data, this.onApiError);
  }

  public login(data: any): Observable<any> {
    return this.http.postCallWithResponse(this.url.login(), data, this.onApiError);
  }
  //eventId optional for filtering purposes
  public getReservations(eventId: number, reserveStatus: number = 0, reservationId?:number): Observable<Reservation[]> {
    return this.http.getCall(this.url.getReservations(eventId, reserveStatus, reservationId), this.onApiError);
  }

  public getReservationStatus() {
    return this.http.getCall(this.url.getReservationStatus(), this.onApiError).pipe(
      map((status: ReservationStatus[]) => {
        status.forEach((x, y) => {
          //console.log("server:" + x.reservationStatusValue)
          //console.log(Object.entries(TranslatedReservationStatusEnum).find(k => k[1][1] == x.reservationStatusValue))
          //x._translatedStatus = Object.entries(TranslatedReservationStatusEnum).find(k => k[1][1] == x.reservationStatusValue)?.[1];
          //console.log(x._translatedStatus)
        })
        return status;
      }));
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

  public getEvents(loadImages:boolean, withReservations:boolean = false) {
    return this.http.getCall(loadImages ? this.url.getEvents():this.url.getEventsNoImages(withReservations), this.onApiError).pipe(
      map((data: EventPartiesViewInfo) => {
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
    return this.http.getCall(this.url.getEventDetail(id), this.onApiError).pipe(
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
   return this.http.postCall(this.url.getEvents(), event, this.onApiError);
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
    return this.http.postCall(this.url.getReservations(0), data, this.onApiError);
  }

  public putReservation(data: ReservationPost) {
    return this.http.putCall(this.url.getReservations(0,0,0,true), data, this.onApiError);
  }

  public getPrCustomers(): Observable<PrCustomerView[]> {
    return this.http.getCall(this.url.getPrCustomers()).pipe(
      map((data:PrCustomerView[]) => {
        data.map((item: PrCustomerView) => {
          item._dropId = item.customerId;
          item._modalDropText = item.surname != null ? item.name + " - " + item.surname : item.name;
          return item;
        })
        return data;
      }));
  }

  getReservationEvent(eventId:any): Observable<Reservation> {
    return this.http.getCall(this.url.getEventReservation(eventId));
  }

  public acceptReservation(reservationId: number, status: number) {
    return this.http.putCall(this.url.getReservations(0) + "?reservationId="+ reservationId.toString() + "&status=" + status.toString(), this.onApiError)
  }

  /*
   * TODO: usare il body per l'invio di dati, non le query params
   */  
  public confirmReservationBudget(reservationId:number, euro: number): Observable<any> {
    return this.http.putCall(this.url.confirmBudget() + `?reservationId=${reservationId}&euro=${euro}`, null, this.onApiError);
  }

  public getPaymentsOverview(userId?: string): Observable<PaymentsOverviewFull> {
      return this.http.getCall(this.url.paymentOverview(userId), this.onApiError).pipe(
        map((data: PaymentsOverviewFull) => {
          data.paymentsOverview.map((item: PaymentOverview) => {
            item.userIdView = "btnDetails_" + item.userId;
            return item;
          })
          return data;
        })
      );        
  }

  public getPaymentsOverviewDetails(userId:string) {
    return this.http.getCall(this.url.paymentOverviewDetails(userId), this.onApiError);
  }

  public postPayment(newPayment: PaymentPost) {
    return this.http.postCall(this.url.reservationsPayments(), newPayment, this.onApiError);
  }

  public getEventReservationTables(eventId?:number) {
    return this.http.getCall(this.url.getEventReservationTables(eventId), this.onApiError);
  }

  public getTables() {
    return this.http.getCall(this.url.getTables(), this.onApiError).pipe(
      map((tables: Table[]) => {
        tables.forEach((x, y) => {
          tables[y]._dropId = x.tableId;
          tables[y]._modalDropText = x.tableAreaDescription + "-" + x.tableNumber;
        })
        return tables;
      }));
  }

  public getAssignableTables(eventId?:number): Observable<Table[]> {
    return this.http.getCall(this.url.getAssignableTables(eventId)).pipe(
      map((values: Table[]) => {
        values.forEach((x, y) => {
          values[y]._dropId = x.tableId;
          values[y]._modalDropText = x.tableAreaDescription + " " + x.tableNumber;
          console.log(x)
        })
        return values;
      })
    );
  }

  public assignTable(data:AssignTablePost) {
    return this.http.postCall(this.url.postAssignTable(), data, this.onApiError);
  }

  public getEventTables(eventId:number):Observable<TableEvents> {
    return this.http.getCall(this.url.getEventTables(eventId));
  }

  public getTableMap<T>(): Observable<TableMapFileInfo> {
    return this.http.getCall(this.url.getTableMap());
  }

  public getCatalogs(): Observable<CatalogView> {
    return this.http.getCall(this.url.getCatalog(), this.onApiError).pipe(
      map((data:CatalogView) => {
        data.catalog?.forEach((item: Catalog) => {
          item._modalDropText = item.catalogName;
          item._dropId = item.catalogId;
        })
        return data;
      }));
  }

  public getProducts(catalogId?:number, isShopProduct:boolean = false): Observable<Product[]> {
    return this.http.getCall(this.url.getProducts(catalogId, isShopProduct), this.onApiError).pipe(
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
    return this.http.postCall(this.url.getProducts(), data, this.onApiError);
  }

  public deleteProduct(productId:number) {
    return this.http.deleteCall(this.url.deleteProduct(productId));
  }
  //In realtà più che una put sarebbe una post perchè crea la testata d'ordine associata al tavolo con le rispettive righe
  public putTablOrder(tableId:number, data:TableOrderPut) {
    return this.http.putCall(this.url.putTableOrder(tableId), data, this.onApiError);
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
