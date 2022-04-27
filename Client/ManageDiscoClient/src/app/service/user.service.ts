import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { CookieConstants } from "../model/models";

@Injectable()
export class UserService {

  _points: BehaviorSubject<number> = new BehaviorSubject<number>(0);

  public userIsAuthenticated() {
    return document.cookie.includes(CookieConstants.ISAUTH_COOKIE + "=1");
  }

  public userIsAdminstrator() {
    return document.cookie.includes(CookieConstants.AUTH_FULL_COOKIE + "=True");
  }

  public userIsCustomer() {
    return document.cookie.includes(CookieConstants.ISAUTH_COOKIE + "=1") &&
      !document.cookie.includes(CookieConstants.AUTH_FULL_COOKIE + "=True") &&
      !document.cookie.includes(CookieConstants.AUTH_STANDARD_COOKIE + "=True");
  }

  public userIsPr() {
    return document.cookie.includes(CookieConstants.AUTH_STANDARD_COOKIE + "=True") && !document.cookie.includes(CookieConstants.AUTH_FULL_COOKIE + "=True");
  }

  public userIsInStaff() {
    return document.cookie.includes(CookieConstants.AUTH_STANDARD_COOKIE + "=True") || document.cookie.includes(CookieConstants.AUTH_FULL_COOKIE + "=True");
  }

  public userIsGuest() {
    return document.cookie.includes(CookieConstants.ISAUTH_COOKIE + "=0");
  }

  public getCustomerPrCode() {
    if (this.userIsCustomer()) {
      var cookies = document.cookie.split(";");
      var prCode = cookies.find(x => x.includes(CookieConstants.PR_REF_COOKIE))?.split("=")[1];
      return prCode;
    } else
      return "";      
  }

  public setPoints(points:number) {
    this._points.next(points);

  }
  public getPoints() {
    return this._points;
  }
}
