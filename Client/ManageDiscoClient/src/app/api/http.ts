import { HttpClient, HttpErrorResponse, HttpHeaders, HttpStatusCode } from "@angular/common/http";
import { ComponentRef} from "@angular/core";
import { ViewContainerRef } from "@angular/core";
import { Inject } from "@angular/core";
import { ComponentFactoryResolver } from "@angular/core";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError } from "rxjs";
import { Observable } from "rxjs";

@Injectable()
export class ApiHttpService {

  modalType: any;
  viewChild?: ViewContainerRef;

  
  constructor(private http: HttpClient,
        private router: Router,
        @Inject("urlRedirect") urlOnUnauthorized: string) {
  }

  public getCall(url: string, onErrorCallback?:(status: number) => any): Observable<any> {
    return this.http?.get(url, { withCredentials: true, observe: 'body'}).pipe(
      catchError(err => {
        if (onErrorCallback != null)
          onErrorCallback(err.status);
        return err;
      }));
  }

  public postCall(url: string, data: any, onErrorCallBack?:(status:number) => any) {
    return this.http?.post(url, data, { withCredentials: true, observe: 'body'}).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
          if (onErrorCallBack != null)
            onErrorCallBack(err.status);
        }
       
        return err;
      }));
  }

  public postCallWithResponse(url: string, data: any, onErrorCallBack?: (status: number) => any) {
    return this.http?.post(url, data, { withCredentials: true, observe: 'response' }).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
          if (onErrorCallBack != null)
            onErrorCallBack(err.status);
        }
        return err;
      }));
  }

  public putCall(url: string, data: any, onErrorCallback?:(status:number) => any) {
    return this.http?.put(url, data, {withCredentials: true}).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
          if (onErrorCallback)
            onErrorCallback(err.status);
        }
        return err;
      }));
  }

  public deleteCall(url: string) {
    return this.http.delete(url, {withCredentials: true}).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
         
        }
        return err;
      }));
  }
}
