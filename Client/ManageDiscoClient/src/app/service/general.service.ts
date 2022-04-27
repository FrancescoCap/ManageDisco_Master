import { Injectable } from "@angular/core";
import { DeviceDetectorService } from 'ngx-device-detector';
import { Subject } from "rxjs";

@Injectable()
export class GeneralService {

  private isMenuLoaded:Subject<boolean> = new Subject<boolean>();

  constructor(private deviceController: DeviceDetectorService) {

  }

  //nei casi in cui sia necessario inserire un id all'interno degli id delle views con questo metodo lo splitto e lo restituisco
  public getIdFromView(viewId:string) {
    var splitted = viewId.split("_");
    return splitted[splitted.length - 1];
  }

  public rewriteDateToISO(value:string):string {
    var formattedDate = "";
    var dateSplit = value.split('/');
    formattedDate = dateSplit[2] + "-" + dateSplit[1] + "-" + dateSplit[0];
  
    return formattedDate;
  }

  public isMobileView() {
    return this.deviceController.isMobile();
  }

  public isTabletView() {
    return this.deviceController.isTablet();
  }
  public isDesktopView() {
    return this.deviceController.isDesktop();
  }

  public isMenuAlreadyLoaded() {    
    if (this.isMenuLoaded.observers.length == 0) {      
      this.setMenuLoaded(false);
      console.log(this.isMenuLoaded);
    }

    return this.isMenuLoaded;
  }

  public setMenuLoaded(state:boolean) {
    this.isMenuLoaded.next(state);
  }
}
