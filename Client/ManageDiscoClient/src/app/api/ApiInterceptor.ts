import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError,Observable, of, throwError } from "rxjs";
import { ApiCaller } from "./api";

@Injectable()
export class ApiInterceptor implements HttpInterceptor {

  /*
   * code source: https://stackoverflow.com/questions/46017245/how-to-handle-unauthorized-requestsstatus-with-401-or-403-with-new-httpclient
   */

  constructor(private _api: ApiCaller, private router:Router) {}

  private handleAuthError(req: HttpRequest<any>, next: HttpHandler, err: HttpErrorResponse): Observable<any> {
    //handle your auth error or rethrow
    if (err.status === 401 || err.status === 403) { 
       this._api.getRefreshToken()
         .subscribe((data: any) => {
           window.location.reload();
          });
      // if you've caught / handled the error, you don't want to rethrow it unless you also want downstream consumers to have to handle it as well.
      return of(); // or EMPTY may be appropriate here
    }
    return throwError(err);
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Clone the request to add the new header.
    const authReq = req.clone();
    // catch the error, make specific functions for catching specific errors and you can chain through them with more catch operators
    return next.handle(authReq).pipe(
      catchError(err =>
        this.handleAuthError(req, next, err))); //here use an arrow function, otherwise you may get "Cannot read property 'navigate' of undefined" on angular 4.4.2/net core 2/webpack 2.70
  }

}
