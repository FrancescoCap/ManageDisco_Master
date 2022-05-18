import { server_URL } from "../app.module";

export class Endpoints {

  private base_url = server_URL;

  exportTables(eventId:number) {
    return this.base_url + `Reservations/Table/PdfExport?eventId=${eventId}`;
  }

  postAutoAssignTable(eventId:any) {
    return this.base_url + `Reservations/AutoAssign?eventId=${eventId}`;
  }

  putUserPermission() {
    return this.base_url + `UserPermission`;
  }

  getUserPermission() {
    return this.base_url + `UserPermission/UserPermissionView`;
  }

  getPermissionAction() {
    return this.base_url + `UserPermission/PermissionAction`;
  }

  getRefreshToken() {
    return this.base_url + `User/RefreshToken`;
  }

  closeFreeEntrance(eventId:any) {
    return this.base_url + `EventParties/FreeEntrance?eventId=${eventId}`;
  }

  checkCouponValidation(code:string) {
    return this.base_url + `Coupon/Validation?couponCode=${code}`;
  }

  getUserAwards() {
    return this.base_url + `ProductShop/UserAwards`;
  }

  purchaseProduct(productId:any) {
    return this.base_url + `ProductShop/Purchase?productId=${productId}`;
  }

  getProductShopType() {
    return this.base_url + `ProductShopTypes`;
  }

  getShop() {
    return this.base_url + `ProductShop`;
  }

  getCollaborators() {
    return this.base_url + `User/Collaborators`;
  }

  getRoles() {
    return this.base_url + `User/Roles`;
  }

  getStatistics(eventId: any): string {
    return this.base_url + `Statistics?eventId=${eventId}`;
  } 

  public confirmUserPhoneNumber(refer:any) {
    return this.base_url + `User/ConfirmPhoneNumber?refer=${refer}`;
  }

  public getSubscription(eventId: any) {   
      return this.base_url + `Whatsapp`;
  }

  public getFreeEntrance(eventId:any) {
    
      return this.base_url + `Coupon/Request?eventId=${eventId}`;
  }

  public getCoupon(refer: any) {   
      return this.base_url + `Coupon?refer=${refer}"`;
  }


  public getCouponInfo(refer: any) {    
    return this.base_url + `Coupon/Validate?couponUserId=${refer}`;
  }

  public logout() {
    return this.base_url + "User/Logout";
  }

  public postContact() {
    return this.base_url + "Contacts"
  }

  public getContact() {
    return this.base_url + "Contacts";
  }
  public getContactTypes() {
    return this.base_url + "ContactTypes";
  }


  public putWarehouseQuantity() {
    return this.base_url + `Warehouses/Product/Add`;
  }

  public getWarehouse() {
    return this.base_url + "Warehouses";
  }

  public getHome() {
    return this.base_url + "Home";
  }

  public getHomeInfo() {
    return this.base_url + "Home/Info";
  }

  public putUserInfo(): string {
    return this.base_url + "User/Edit";
  }

  public getUserReservation(): string {
    return this.base_url + `User/Reservation`;
  }

  public getUserInfoFromId(userId:any) {
    return this.base_url + `Coupon?userId=${userId}`;
  }

  public getUserInfo(): string {
    return this.base_url + `User/Profile`;
  }

  public getMenu(): string {
    return this.base_url + `Menu`;
  }

  public register(): string {
    return this.base_url + "User/Register";
  }

  public login(): string {   
      return this.base_url + "User/Login"
  }

  public getReservations(eventId: number, reserveStatus?:number, reservationId?:number, edit:boolean = false): string {
    if (eventId > 0 && reserveStatus != 0)
      return this.base_url + `Reservations/Filter/Event?eventId=${eventId}&resStatus=${reserveStatus == 0 ? 2 : reserveStatus}`;
    else if (eventId > 0)
      return this.base_url + `Reservations?eventId=${eventId}`;
    else if (reservationId != null && reservationId > 0)
      return this.base_url + `Reservations?id=${reservationId}`;
    else if (edit)
      return this.base_url + `Reservations/Edit`;
    else
      return this.base_url + `Reservations`;
  }

  public getEventReservation(eventId:any) {
    return this.base_url + `Reservations/Event?eventId=${eventId}`
  }

  public getPrCustomers(prCode?:string) {
    return this.base_url + `PrCustomers?prCode=${prCode}`;
  }

  public getAcceptedReservations(eventId:number): string {
    return this.base_url + `Reservations/Table${eventId}`;
  }

  public getEvents(eventId?: any): string {
    if (eventId != null)
      return this.base_url + `EventParties?eventId=${eventId}`;
    else
      return this.base_url + `EventParties`;
  }

  public getEventDetail(eventId:number) {
    return this.base_url + `EventParties/Details?eventId=${eventId}`;
  }

  public putCancelEvent(eventId: number) {
    return this.base_url + `EventParties/Cancel?eventId=${eventId}`;
  }

  public editEventDetails(eventId: number) {
    return this.base_url + `EventParties/Details/Edit?eventId=${eventId}`;
  }

  public getReservationTpe() {
    return this.base_url + "Reservations/Type";

  }
  public confirmBudget() {
    return this.base_url + `Reservations/Budget/Confirm`;
  }

  public paymentOverview(userId?:string) {
    return this.base_url + `PaymentOverviews?UserId=${userId}`;
  }

  public paymentOverviewDetails(userId:string) {
    return this.base_url + `ReservationPayments/User?userId=${userId}`;
  }

  public reservationsPayments() {
    return this.base_url + `ReservationPayments`;
  }

  public getReservationStatus() {
    return this.base_url + `ReservationStatus`;
  }

  public getTables() {
    return this.base_url + `Tables`;
  }

  public getAssignableTables(eventId?:number) {
    return this.base_url + `Tables/Assignable?eventId=${eventId}`;
  }

  public getEventTables(eventId:number) {
    return this.base_url + `Tables/TablesOrder?eventId=${eventId}`;
  }

  public postAssignTable() {
    return this.base_url + `Reservations/AssignTable`;
  }

  public getTableMap() {
    return this.base_url + `Tables/Map`;
  }

  public getCatalog() {
    return this.base_url + "Catalogs";
  }

  public getProducts(catalogId?: number, isShopProduct?:boolean) {
    if (catalogId != null && catalogId > 0)
      return this.base_url + `Products?catalogId=${catalogId}&shop=${isShopProduct}`;
    else
      return this.base_url + `Products?shop=${isShopProduct}`;
  }

  public deleteProduct(productId:number) {
    return this.base_url + `Products?productId=${productId}`;
  }

  public putTableOrder(tableId?:number):string {
    return this.base_url + `Tables/TableOrder?tableId=${tableId}`
  }

  public getTableOrderRow(tableId:number) {
    return this.base_url + `Tables/TableOrdersList?tableId=${tableId}`
  }
}
