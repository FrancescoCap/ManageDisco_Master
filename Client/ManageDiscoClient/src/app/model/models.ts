import { Data } from "@angular/router";

export enum ModalType {
  NEW_RESERVATION = 1,
  NEW_PAYMENT = 2,
  NEW_PRODUCT = 3,
  NEW_TABLEORDER = 4,
  ERROR = 5,
  LISTVIEW = 6
}

export interface HomeInfo {
  events?: EventParty[];
}

/*******
 * Oggetto di test per impostazione menu locale senza riceverlo dal server
 ********/
export interface NavigationLabel {
  label?: string;
  isActive?: boolean;
  link?: string;
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
}

export interface HeaderMenu {
  header?: string;
  link?: string;
  child?:ChildMenu[]
}

export interface ChildMenu {
  title?: string;
  link?: string;
}

export interface User {
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
  prCode: string;
}

export interface LoginRequest {
  email?: string;
  password?: string;
}

export interface AuthResponse {
  token?: string;
  refreshToken?: string;
  message?: string;
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
  eventPartyId?: number;
  reservationPeopleCount?: number;
  reservationUserCodeValue?: string;
  reservationType?: number;
  reservationExpectedBudget?: number;
  reservationRealBudget?: number;
  reservationName?: string;
  reservationOwnerId?: string;
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
}

export interface ReservationPayments {
  reservationPymentId: number;
  userId?: string;
  reservationPaymentAmount?: number;
  reservationPaymentDate?: Date;
}

export interface ReservationStatus {
  reservationStatusId?: number;
  reservationStatusValue?: string;
}

export interface PaymentPost {
  userId?: string;
  reservationPaymentAmount?: number;
}

export interface Table {
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

}

