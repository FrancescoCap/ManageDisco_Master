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

  //private handleAuthError(req: HttpRequest<any>, next: HttpHandler, err: HttpErrorResponse): Observable<any> {
  //  //save last visited page
  //  localStorage.setItem(LOCALSTORARE_LAST_PAGE_REQUEST, req.url);
  //  //handle your auth error or rethrow
  //  if (err.status === 401) {
  //    this._api.getRefreshToken().pipe(        
  //      catchError(err => {
  //        this.cookieService.deleteAll();
  //        this.router.navigateByUrl("/Login");
  //        return err;
  //      }))
  //       .subscribe((data: any) => {
  //         window.location.reload();
  //        });
  //    // if you've caught / handled the error, you don't want to rethrow it unless you also want downstream consumers to have to handle it as well.
  //    return of(); // or EMPTY may be appropriate here
  //  }
  //  return throwError(err);
  //}

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
          this.router.navigateByUrl("/Login");
          this._api.logout();
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
