import { Injectable } from "@angular/core";

@Injectable()
export class TableService {

  public setPagination(maxRow:number, items:any[]) {
    var pages = items.length / maxRow;

  }
}
