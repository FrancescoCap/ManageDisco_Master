import { Injectable } from "@angular/core";

@Injectable()
export class GeneralService {

  //nei casi in cui sia necessario inserire un id all'interno degli id delle views con questo metodo lo splitto e lo restituisco
  public getIdFromView(viewId:string) {
    var splitted = viewId.split("_");
    return splitted[splitted.length - 1];
  }

  public rewriteDateToISO(value:string):string {
    var formattedDate = "";
    var dateSplit = value.split('/');
    formattedDate = dateSplit[2] + "-" + dateSplit[1] + "-" + dateSplit[0];
    console.log(formattedDate)
    return formattedDate;
  }
}
