import { DatePipe } from "@angular/common";

export class DateHelper {

  public formatDate(date:Date, format:string): any {
    var pipe = new DatePipe("en-US");   
    return pipe.transform(date, 'short');
  }

}
