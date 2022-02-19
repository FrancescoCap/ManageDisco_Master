import { Injectable } from "@angular/core";

@Injectable()
export class UserService {

  public userIsAuthenticated() {
    return document.cookie.includes("isAuth=1");
  }

  public userIdAdminstrator() {
    return document.cookie.includes("authorization_full=True");
  }

  public userIsCustomer() {
    return document.cookie.includes("isAuth=1") && !document.cookie.includes("authorization_full=True") && !document.cookie.includes("authorization=True");
  }

  public userIsPr() {
    return document.cookie.includes("authorization=True") && !document.cookie.includes("authorization_full=True");
  }

  public userIsInStaff() {
    return document.cookie.includes("authorization=True") || document.cookie.includes("authorization_full=True");
  }

  public userIsGuest() {
    return document.cookie.includes("isAuth=0");
  }

  public getCustomerPrCode() {
    if (this.userIsCustomer()) {
      var cookies = document.cookie.split(";");
      var prCode = cookies.find(x => x.includes("pr_ref"))?.split("=")[1];
      return prCode;
    } else
      return "";
      
  }

}
