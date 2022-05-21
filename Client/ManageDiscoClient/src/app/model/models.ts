export enum CookieConstants {
  ISAUTHENTICATED = "authenticated",
  AUTHORIZATION_COOKIE = "c_us",
  REFRESH_COOKIE = "c_r",
  ISAUTH_COOKIE = "c_in",
  PR_REF_COOKIE = "p_r",
  AUTH_FULL_COOKIE = "a_f",
  AUTH_STANDARD_COOKIE = "a_s",
  CLIENT_SESSION = "c_s"
}

export enum TranslatedRolesEnum {
  WAREHOUSE_WORKER = "Magazziniere",
  PR = "Pr",
  ADMINISTRATOR = "Amministratore"
}

export interface CouponValidation {
  products?: CouponValidationRow[];
}

export interface CouponValidationRow {
  _extraDescriptionToAdd?: string;

  productId?: number;
  productName?: string;
  productQuantity?: number;
}

export interface UserProduct {
  userProductId?: number;
  userId?: string; 
  productShopId?: number;
  userProductUsed?: boolean;
  productShopHeader?: ProductShopHeader;
  userProductCode?: string;
}

export interface ProductShopType {
  _dropId?: any;
  _modalDropText?: any;
  _valueOut?: any;

  productShopTypeId?: number;
  productShopTypeDescription?: string;
}

export interface ProductShopHeader {
  productShopHeaderIdId?: number;
  productShopHeaderPrice?: number;
  productShopHeaderName?: string;
  productShopHeaderDescription?: string;
  productShopHeaderProductId?: number;
  productShopProductQuantity?: number;
  productShopTypeId?: number;
  productShopBase64Image?: string;
  rows?: ProductShopRow[];
}

export interface ProductShopRow {
  productShopRowId?: number;
  productId?: number; 
  productShopRowQuantity?: number;
  productShopHeaderId?: number;
}

export enum TranslatedReservationStatusEnum {
  REJECTED = "Riufiutato",
  APPROVED = "Approvato",
  PENDING = "In attesa"
}

export interface Role {
  _translatedRole?: string;
  id: string;
  name: string;
}

export interface Statistics {
  freeEntrance?: FreeEntrancePercentage;
  eventTable?: EventTableOrder;
}

export interface FreeEntrancePercentage {
  couponSent?: number;
  couponValidated?: number;
  couponList?: FreeEntrance[];
}
export interface FreeEntrance {
  name?: string;
  surname?: string;
  validated?: boolean;
}

export interface EventTableOrder {
  totalOrderTable?: number;
  tableCount?: number;
  peopleCountFromTable?: number;
  tableCoupons?: EventTableOrderCoupon[];
}

export interface EventTableOrderCoupon {
  tableName?: string;
  couponCode?: string;
  couponDescription?: string;
}

export enum ModalType {
  NEW_RESERVATION = 1,
  NEW_PAYMENT = 2,
  NEW_PRODUCT = 3,
  NEW_TABLEORDER = 4,
  ERROR = 5,
  LISTVIEW = 6,
  LOGIN = 7,
  MESSAGE = 8,
  SUCCESS = 9,
  INFO = 10
}

export interface DiscoEntity {
  discoId?: string;
  discoName?:string
  discoVatCode?: string;
  discoAddress?: string;
  discoCity?: string;
  discoProvince?: string;
  discoCityCap?: string;
  discoOpeningTime?: string;
  discoClosingTime?: string;
}

export interface Contact {
  contactId?: number;
  contactTypeId?: number;
  contactDescription?: string;
  contactTypeDescription?: string;
}


export interface ContactType {
  contactTypeId?: number;
  contactTypeDescription?: string;
}

export interface ContactGroup {
  contactTypeDescription?: string;
  contactTypeId?: number;
  contactsValues?: string[];
}

export interface Warehouse {
  productId?: number;
  productName?: string;
  warehouseQuantity?: number;
}

export interface HomeInfo {
  events?: EventParty[];
  homePhoto?: Home[];
  photoType?: PhotoType[];
  contacts?: ContactGroup[]
  discoEntity?: DiscoEntity;
}

export interface Home {
  photoTypeDescription?: string;
  base64Image?: string[];
  homePhotoPath?: string;
}

export interface HomePhotoPost {
  homePhotoBase64?: string;
  photoTypeId?: number;
  photoName?: string;
}

export interface HomePhotoPut {
  photoName?: string;
  base64NewPhoto?: string;
}

export interface PhotoType {
  photoTypeId?: number;
  photoTypeDescription?: string;
  photoTypeMaxNumber?: number;
}

/*******
 * Oggetto di test per impostazione menu locale senza riceverlo dal server
 ********/
export interface NavigationLabel {
  index?: number;
  label?: string;
  isActive?: boolean;
  link?: string;
  id?: string;
  child: NavigationLabelChild[];
}

export interface NavigationLabelChild {
  index?: number;
  label?: string;
  isActive?: boolean;
}

export interface UserInfoPut {
  name?: string;
  surname?: string;
  email?: string;
  phoneNumber?: string;
}

export interface UserInfoView {
  userName?: string;
  userSurname?: string;
  userEmail?: string;
  isCustomer?: boolean;
  prName?: string;
  prSurname?: string;
  prEmail?: string;
  prCode?: string;
  userPhoneNumber?: string;
  isPhoneNumberConfirmed?: string;
  invitationLink?: string;
  userPoints?: number;
}

export interface HeaderMenu {
  header?: string;
  link?: string;
  icon?: string;
  child?:ChildMenu[]
}

export interface ChildMenu {
  title?: string;
  link?: string;
  icon?: string;
}

export interface User {
  id?: string;
  name?: string;
  surname?: string;
  reservationsCount?: string;
  reservaionCode?: string;
  grossCredit?: string;
  netCredit?: string;
}
export interface RegistrationRequest {
  email: string;
  password: string;
  username: string;
  name: string;
  surname: string;
  phoneNumber: any;
  prCode?: string;
  gender?: any;
  role?: any;
}

export interface LoginRequest {
  email?: string;
  password?: string;
}

export interface LoginResponse {
  token?: string;
  refreshToken?: string;
  message?: string;
  userPoints?: number;
  userNameSurname?: string;
  operationSuccess?: boolean;
}

export interface Reservation {
  _dropId?: any;
  _dropText?: any;
  _reservationDateFormatted?: Date;

  reservationId?: number;
  reservationCode?: string;
  eventId?: number;
  eventName?: string;
  reservationUserCode?: string;
  reservationTypeValue?: string;
  reservationPeopleCount?: number;
  reservationDate?: Date;
  reservationExpectedBudget?: number;
  reservationRealBudget?: number;
  canAcceptReservation?: boolean;
  canAcceptBudget?: boolean;
  reservationStatusId?: number;
  reservationStatus?: string;
  reservationTablAssigned?: string;
  reservationName?: string;
  tableId?: number;
  reservationTableName?: string;
}

export interface ReservationViewTable {
  canAssignTable?: boolean;
  reservations?: Reservation[];
}

export interface ReservationPost {
  reservationId?: number;
  eventPartyId?: number;
  reservationPeopleCount?: number;
  reservationUserCodeValue?: string;
  reservationType?: number;
  reservationExpectedBudget?: number;
  reservationRealBudget?: number;
  reservationName?: string;
  reservationOwnerId?: string;
  tableId?: number;
}

export interface PrCustomerView {
  _dropId?: any;
  _modalDropText?: any;

  customerId?: any;
  name?: string;
  surname?: string;
}

export interface EventPartyView {
  userCanAddReservation?: boolean;
  userCanAddEvent?: boolean;
  userCanDeleteEvent?: boolean;  
  events?: EventParty[];
}

export interface UserPermission {
  userId?: string;
  userPermissionId?: number;
  name?: string;
  surname?: string;
  permissionActionId?: number;
  permissionActionDescription?: string;
  permissionActionAllowed?: boolean;
}

export interface UserPermissionPut {
  userId?: string;
  permissionId?: number;
}

export interface UserPermissionTable {
  userPermissionTableHeaderCol?: string[];
  rows?: UserPermissionRow[];
}

export interface UserPermissionRow {
  user?: string;
  userPermissionTableCell?: UserPermissionCell[];
}

export interface UserPermissionCell {
  _stateString?: string;

  userId?: string;
  userIdentity?: string;
  permissionId?: number;
  permissionState?: boolean;
}

export interface PermissionAction {
  permissionActionId?: number;
  permissionActionDescription?: string;
}

export interface EventParty {
  _showDate?: boolean;
  _dropId?: any;
  _modalDropText?: any;

  id?: number;
  name?: string;
  description?: string;
  maxAge?: number;
  date?: Date;
  eventPartyStatusDescription?: string;
  linkImage?: any[];
  imagePreview?: any;
  entrancePrice?: any;
  tablePrice?: any;
  freeEntranceDescription?: any;
  userHasReservation?: boolean;
  userCanEditInfo?: boolean;
  userCanEnrollFreeEntrance?: boolean;
  eventIsEnd?: boolean;
  freeEntranceEnabled?: boolean;
}

export interface ReservationType {
  _dropId?: any;
  _modalDropText?: any;

  reservationTypeId?: number;
  reservationTypeString?: string;
}

export interface PaymentOverview {
  userIdView?:string,

  paymentId?: number;
  userId?: string;
  name?: string;
  surname?: string;
  totalIncoming?:number,
  totalPayed?:number,
  resumeCredit?: number;
  paymentDescription?: string;
}

export interface ReservationPayments {
  reservationPymentId: number;
  userId?: string;
  paymentDescription?: string;
  reservationPaymentAmount?: number;
  reservationPaymentDate?: Date;
  reservationPaymentDescription?: string;
}

export interface ReservationStatus {
  _translatedStatus?: string;
  reservationStatusId?: number;
  reservationStatusValue?: string;
}

export interface PaymentPost {
  userId?: string;
  reservationPaymentAmount?: number;
  reservationPaymentDescription?: string;
}

export interface Table {
  _dropId?: any;
  _modalDropText?: string;
  _assigned?: boolean;

  tableId?: number;
  tableAreaDescription?: string;
  tableNumber?: string;
  discoEntityId?: string;
}

export interface TableEventHeader {
  eventId?: number;
  tables: TableEvents[];
}

export interface TableEvents extends Table {
  eventId?: number;
  tableName?: string;
  tableDate?: Date;
  hasOrder?: boolean;
}

export interface TableOrderHeader {
  tableOrderHeaderId?: number;
  tableId?: number;
  tableOrderHeaderSpending?: number;
  tableOrderHeaderExit?: number;
  rows?: TableOrderRow[];
}

export interface TableOrderRow {
  _modalDropText?: any;

  tableOrderRowId?: number;
  tableOrderRowQuantity?: number;
  productId?: number;
  productName?: string;
  tableOrderHeaderId?: number;
}

export interface TableOrderPut {
  productsId?: {[key:number]: number}//Map<number,number>;
  productsSpendingAmount?: number;
  exitChanged?: number;
  eventId?: number;
  shopCoupon?: string;
}

//Modello per l'assegnazione di un tavolo fisico alla prenotazione
export interface AssignTablePost {
  tableId?: number;
  reservationId?: number;
  eventId?: number;
}

export interface TableMapFileInfo {
  fileName?: string;
  path?: string;
}

export interface CatalogView {
  userCanEditCatalog?: boolean;
  catalog?: Catalog[];
}

export interface Catalog {
  _modalDropText?: any;
  _dropId?: any;

  catalogId?: number;
  catalogName?: string;
}

export interface Product {
  _dropId?: any;
  _modalDropText?: any;
  _valueOut?: any;

  productId?: number;
  productPrice?: number;
  productName?: string;
  catalogId?: number;
  catalogName?: string;
}

export interface NewTableOrderData {
  productsId: {[key:number]:number} // Map<number, number>;
  totSpending?: number;
  totExit?: number;
  shopCoupon?: string;

}

