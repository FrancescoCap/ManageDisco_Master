import { Component, Input, OnInit, Output } from '@angular/core';
import { EventEmitter } from '@angular/core';
import { TableViewDataModel } from './tableview.model';

@Component({
  selector: 'app-tableview',
  templateUrl: './tableview.component.html',
  styleUrls: ['./tableview.component.scss']
})
export class TableviewComponent implements OnInit {

  private readonly LOCALSTORAGE_LAST_PAGE = "lastPage";

  @Input("data-list") dataList: TableViewDataModel = { headers: [], rows: [] };
  @Input("max-rows-page") maxRowsPage: number = 10;

  tablePages: number = 1;
  pagesArray: number[] = [];
  startRowIndex: number = 0;
  selectedPage: number = 1;  

  constructor() { }

  ngOnInit(): void {
    this.setPagination();
    
  }

  setPagination() {
    if (this.pagesArray.length > 0)
      this.pagesArray = [];

    this.selectedPage = 1;
    this.startRowIndex = this.maxRowsPage;
    this.tablePages = this.dataList.rows.length / this.maxRowsPage;
    for (var i = 0; i < this.tablePages; i++) {
      this.pagesArray.push(i+1);
    }
    //Commentandolo perdo la capacità di rimanere sulla stessa pagina dopo un refresh di pagina conseguente ad un ipotetica chimata POST che prevede l'aggiornamento delle righe
    //this.setFocusLastVisitedPage();
    this.startOnDataListChangeListener();
  }

  startOnDataListChangeListener() {
    this.dataList.onDataListChange?.subscribe((newList: TableViewDataModel) => {
      this.dataList = newList;
      console.log(this.selectedPage)
      this.setPagination();
    });
  }

  setFocusLastVisitedPage() {
    var localStoragePage = localStorage.getItem(this.LOCALSTORAGE_LAST_PAGE)
    if (localStoragePage != null) {
      this.selectedPage = Number.parseInt(localStoragePage);
      this.startRowIndex = this.maxRowsPage * this.selectedPage;
    }      
  }

  onPageChange(page: number) {
    this.startRowIndex = this.maxRowsPage * page;
    this.selectedPage = page;
    localStorage.setItem(this.LOCALSTORAGE_LAST_PAGE, page.toString());
  }

  onIconClick(callback: Function, referenceRowId: any) {
    var id = referenceRowId.toString().split("_")[referenceRowId.toString().split("_").length - 1];
    callback(id);
  }

}
