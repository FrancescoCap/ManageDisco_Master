import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { catchError, Observable } from "rxjs";

export class ApiInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    if (!req.url.includes("Login")) {
        var cookie = document.cookie;
        var jwt = cookie.split("=")[1].replace("; auth_consent", "");

        if (!jwt) {
          return next.handle(req);
        }

         const reqCloned = req.clone({
          headers: req.headers.set("Authorization", `Bearer ${jwt}`)
        });

        return next.handle(reqCloned);
    }
    
    return next.handle(req);
  }

  handleError() {

  }

}
