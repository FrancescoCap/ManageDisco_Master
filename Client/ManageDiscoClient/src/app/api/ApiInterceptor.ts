import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError,filter,finalize,forkJoin,mergeMap,Observable, of, Subject, take, throwError } from "rxjs";
import { ApiCaller } from "./api";
import { CookieService } from "ngx-cookie-service";
import { LOCALSTORARE_LAST_PAGE_REQUEST } from "../app.module";
import { UserService } from "../service/user.service";
import { switchMap } from "rxjs";

@Injectable()
export class ApiInterceptor implements HttpInterceptor {

  /*
   * code source: https://stackoverflow.com/questions/46017245/how-to-handle-unauthorized-requestsstatus-with-401-or-403-with-new-httpclient
   */

  isRefreshingToken = false;
  tokenSubject = new Subject<any>();

  constructor(private _api: ApiCaller,
    private router: Router,
    private cookieService: CookieService) { }

  
  private handleAuthError(req: HttpRequest<any>, next: HttpHandler, err: HttpErrorResponse) {
    if (!this.isRefreshingToken) {
      this.isRefreshingToken = true;
      this.tokenSubject.next(null);
      //start token refresh
      return this._api.getRefreshToken().pipe(
        switchMap(value => {
          this.tokenSubject.next("OK");
          return next.handle(req);
        }),
        catchError(err => {
          if (req.url.includes("RefreshToken")) {
            this.router.navigateByUrl("/Login");
            this._api.logout();
          }
         
          return of();
        }),
        finalize(() => {
          this.isRefreshingToken = false;
        })
      )
    } else {
      return this.tokenSubject.pipe(
        filter(token => token != null),
        take(1),
        switchMap(token => {
          return next.handle(req);
        }));
    }
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Clone the request to add the new header.
    const authReq = req.clone();
    // catch the error, make specific functions for catching specific errors and you can chain through them with more catch operators
    return next.handle(authReq).pipe(
      catchError(err => {
        if (err.status == 401) {
          return this.handleAuthError(req, next, err);
        }
        return throwError(err);
      })
    ); //here use an arrow function, otherwise you may get "Cannot read property 'navigate' of undefined" on angular 4.4.2/net core 2/webpack 2.70
  }
}
