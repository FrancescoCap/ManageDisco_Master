import { Inject, Injectable } from "@angular/core";
import { inject } from "@angular/core/testing";
import { CookieService } from "ngx-cookie-service";
import { Subject } from "rxjs";
import { BehaviorSubject } from "rxjs";
import { COOKIE_ISAUTHENTICATED, COOKIE_PR_CODE } from "../app.module";
import { CookieConstants } from "../model/models";

@Injectable()
export class UserService {

  _points: BehaviorSubject<number> = new BehaviorSubject<number>(0);

  constructor(private _cookieService: CookieService) {

  }
 
  
  public userIsAuthenticated() {
    return this._cookieService.get(COOKIE_ISAUTHENTICATED) == "true";
  }

  public setUserAuthenticated() {
    this._cookieService.set(COOKIE_ISAUTHENTICATED, "true");
  }

  public setPrCode(prCode:string) {
    this._cookieService.set(COOKIE_PR_CODE, prCode);
  }

  public getCustomerPrCode() {
    return localStorage.getItem(COOKIE_PR_CODE);
  }

  public setPoints(points:number) {
    this._points.next(points);

  }
  public getPoints() {
    return this._points;
  }
  public removeLocalCookie(cookieName:string) {
    this._cookieService.delete(cookieName);
  }

  
}
