import { HttpClient, HttpErrorResponse, HttpStatusCode } from "@angular/common/http";
import { ComponentRef} from "@angular/core";
import { ViewContainerRef } from "@angular/core";
import { Inject } from "@angular/core";
import { ComponentFactoryResolver } from "@angular/core";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError } from "rxjs";
import { Observable } from "rxjs";
import { ModalService } from "../service/modal.service";

@Injectable()
export class ApiHttpService {

  modalType: any;
  viewChild?: ViewContainerRef;
  
  constructor(private http: HttpClient,
        private router: Router,
        @Inject("urlRedirect") urlOnUnauthorized: string) {
  }

  public getCall(url: string, onErrorCallback?:(status: number) => any): Observable<any> {
    return this.http?.get(url).pipe(
      catchError(err => {
        if (onErrorCallback != null)
          onErrorCallback(err.status);
        return err;
      }));
  }

  public postCall(url: string, data: any, onErrorCallBack?:(status:number) => any) {
    return this.http?.post(url, data).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
          if (onErrorCallBack != null)
            onErrorCallBack(err.status);
        }
        return err;
      }));
  }

  public putCall(url: string, data: any, onErrorCallback?:(status:number) => any) {
    return this.http?.put(url, data).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
          if (onErrorCallback)
            onErrorCallback(err.status);
        }
        return err;
      }));
  }

  public deleteCall(url: string) {
    return this.http.delete(url).pipe(
      catchError(err => {
        if (err.status == HttpStatusCode.Unauthorized) {
         
        }
        return err;
      }));
  }


  public setModalType<T>(type: T) {
    this.modalType = type;
  }

  public setModalContainer(viewChild:ViewContainerRef) {
    this.viewChild = viewChild;
  }
}
