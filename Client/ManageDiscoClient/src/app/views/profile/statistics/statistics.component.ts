import { EventEmitter, Output } from '@angular/core';
import { Component, Input, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { ApiCaller } from '../../../api/api';
import { maxTablePageRows } from '../../../app.module';
import { TableViewDataModel } from '../../../components/tableview/tableview.model';
import { Statistics } from '../../../model/models';

@Component({
  selector: 'app-event-statistics',
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.scss', '../../../app.component.scss']
})
export class StatisticsComponent implements OnInit {

  @Output("onEventChange") onEventChange: EventEmitter<number> = new EventEmitter<number>();
  @Input("eventId") changeEventListener?: Subject<number>;
  @Input("isMobileTemplate") isMobileTemplate = false;

  rowsPerPage = 0;
  statisticsList: Statistics = {};
  statisticsFreeEntranceTableData: TableViewDataModel = {
    headers: [
      {value: "Nome"},
      {value: "Cognome"},
      {value: "Validato"}
    ], rows: []
  };
  statisticsTableCouponTableData: TableViewDataModel = {
    headers: [
      { value: "Tavolo" },
      { value: "Codice coupon" },
      { value: "Descrizione coupon" }
    ], rows: []
  };

  isLoading = false;
  eventId = 0;

  constructor(private _api:ApiCaller) { }

  ngOnInit(): void {
    this.rowsPerPage = maxTablePageRows;
    this.changeEventListener?.subscribe((id: number) => {
      this.eventId = id;
      this.initData();
    })
   
  }

  initData() {
    this.isLoading = true;
    this._api.getStatisticsForEvent(this.eventId)
      .subscribe((data: Statistics) => {
        this.statisticsList = data;
        this.setDataForTableView();
        this.isLoading = false;
      })
  }

  setDataForTableView() {
    this.setTableCouponFreeEntrance();
    this.setTableCouponInOrder();
  }

  setTableCouponFreeEntrance() {
    if (this.statisticsFreeEntranceTableData.rows.length > 0)
      this.statisticsFreeEntranceTableData.rows = [];

    this.statisticsList.freeEntrance?.couponList?.forEach((x, y) => {
      this.statisticsFreeEntranceTableData.rows.push({
        cells: [
          { value: x.name },
          { value: x.surname },
          {
            icon: [
              { isToShow: x.validated, isImage: true, imagePath: x.validated ? '../../../../assets/circle_red_full.png' : '../../../../assets/circle_green_full.png' }
            ]
          },
        ]
      });
    });
  }

  setTableCouponInOrder() {
    if (this.statisticsTableCouponTableData.rows.length > 0)
      this.statisticsTableCouponTableData.rows = [];

    this.statisticsList.eventTable?.tableCoupons!.forEach((x, y) => {
      this.statisticsTableCouponTableData.rows.push({
        cells: [
          {value: x.tableName},
          {value: x.couponCode},
          {value: x.couponDescription},
        ]
      })
    })
  }

}
