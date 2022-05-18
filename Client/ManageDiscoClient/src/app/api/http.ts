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

  private GET: string = "GET";
  private GET_FILE: string = "GET_FILE";
  private POST: string = "POST";
  private PUT: string = "PUT";
  private DELETE: string = "DELETE";  
  
  constructor(private http: HttpClient,
        private router: Router) {
  }

  public getCall(url: string, onErrorCallback?: (status: number, message: string) => any): Observable<any> {
     return this.initCall(this.GET, url, null, onErrorCallback, 'body');
  }

  public getFileCall(url: string, onErrorCallback?: (status: number, message: string) => any): Observable<any> {
    return this.initCall(this.GET_FILE, url, null, onErrorCallback);
  }

  public postCall(url: string, data: any, onErrorCallBack?: (status: number, message: string) => any) {
    return this.initCall(this.POST, url, data, onErrorCallBack, 'body');
  }

  public postCallWithResponse(url: string, data: any, onErrorCallBack?: (status: number, message:string) => any) {
    return this.initCall(this.POST, url, data, onErrorCallBack, 'response');
  }

  public putCall(url: string, data: any, onErrorCallBack?:(status:number, message:string) => any) {
    return this.initCall(this.PUT, url, data, onErrorCallBack, 'body');
  }

  public deleteCall(url: string) {
    return this.initCall(this.DELETE, url, null, undefined, 'body');
  }

  private initCall(method: string, url: string, data?: any, onErrorCallback?: (status: number, message: string) => any, responseObserver?:any):Observable<any> {
    var methodCall: Observable<any> = new Observable<any>();
      
    switch (method) {
      case this.GET:        
        methodCall = this.http.get(url, { withCredentials: true, observe: responseObserver != null ? responseObserver : 'body' }).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
      case this.GET_FILE:
        methodCall = this.http.get(url, { withCredentials: true, responseType: 'blob'}).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
      case this.POST:
        methodCall = this.http.post(url, data, { withCredentials: true, observe: responseObserver != null ? responseObserver : 'body' }).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
      case this.PUT:
        methodCall = this.http.put(url, data, { withCredentials: true, observe: responseObserver != null ? responseObserver : 'body' }).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
      case this.DELETE:
        methodCall = this.http.delete(url, { withCredentials: true, observe: responseObserver != null ? responseObserver : 'body' }).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
      default:
        methodCall = this.http.get(url, { withCredentials: true, observe: responseObserver != null ? responseObserver : 'body' }).pipe(catchError(err => this.handleErrorResponse(err, onErrorCallback)));
        break;
    }    
    
    return methodCall;
  }

  private handleErrorResponse(err: any, onErrorCallback?: (status: number, message: string) => any): any {    
    if (onErrorCallback != null) {
      if (err.status == HttpStatusCode.Unauthorized)
        onErrorCallback(err.status, err.message);
      else
        onErrorCallback(err.status, err.error.message);
    }
    return err;
  }
}
