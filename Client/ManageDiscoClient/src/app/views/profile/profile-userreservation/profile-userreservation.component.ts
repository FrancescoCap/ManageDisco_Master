import { formatDate } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { ApiCaller } from '../../../api/api';
import { TableViewDataModel } from '../../../components/tableview/tableview.model';
import { Reservation } from '../../../model/models';

@Component({
  selector: 'app-profile-userreservation',
  templateUrl: './profile-userreservation.component.html',
  styleUrls: ['./profile-userreservation.component.scss', '../../../app.component.scss']
})
export class ProfileUserreservationComponent implements OnInit {

  @Input("isMobileTemplate") isMobileTemplate = false;

  reservationTableList: TableViewDataModel = {
    headers: [
      { value:"Data prenotazione"},
      { value:"Nome prenotazione"},
      { value:"Evento"},
      { value:"Budget"}
    ], rows: []
  }

  reservation: Reservation[] = [];

  isLoading = false;
  maxTablePageRows = 10;
  

  constructor(private _api:ApiCaller) { }

  ngOnInit(): void {
    this.initData();
  }

  initData() {
    this.isLoading = true;
    this._api.getUserReservation()
      .subscribe((data: Reservation[]) => {
        this.reservation = data;
        this.setDataForTableView(this.reservation);
        this.isLoading = false;
      })
  }

  setDataForTableView(list:Reservation[]) {
    list.forEach((x, y) => {
      this.reservationTableList.rows.push({
        cells: [
          {value: formatDate(x.reservationDate!.toString(), "dd/MM/yyyy", "en-US")},
          {value: x.reservationName},
          {value: x.eventName},
          {value: x.reservationRealBudget, isCurrency: true}
        ]
      })
    })

  }

}
