import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { AssignTablePost, EventParty, EventPartyView, Reservation, ReservationStatus, ReservationViewTable, Table, TableMapFileInfo } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss', '../../app.component.scss']
})
export class TableComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  acceptedReservations: ReservationViewTable = {};
  events?: EventPartyView;
  reservationStatus: ReservationStatus[] = [];
  avaiableTables: Table[] = [];

  selectedEventId: number = 0;
  selectedResStatusId: number = 0;
  isLoading = false;

  constructor(private api: ApiCaller,
      private _modal:ModalService  ) { }

  ngOnInit(): void {
    this.initData();
  }

  ngAfterViewInit() {
    this.api.setModalContainer(this.modalContainer!);
  }

  initData() {
    this.isLoading = true;

    const calls: Observable<any>[] = [
      this.api.getEvents(),
      this.api.getReservations(0, 0),
      this.api.getReservationStatus(),
      this.api.getAssignableTables(0)
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message);
        return err;
      })).subscribe((data: any) => {
        this.events = data[0];
        this.acceptedReservations = data[1];
        this.reservationStatus = data[2];
        this.avaiableTables = data[3];
        this.isLoading = false;
      })
  }

  onEventChange(event: any) {
    this.selectedEventId = event.target.value;
    this.loadReservation();
  }

  onResStatusChange(event: any) {
    this.selectedResStatusId = event.target.value;
    this.loadReservation();
  }

  loadReservation() {
    forkJoin([
      this.api.getReservations(this.selectedEventId, this.selectedResStatusId),
      this.api.getAssignableTables(this.selectedEventId)
    ]).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data: any) => {
        console.log(data[0])
        this.acceptedReservations = data[0];
        this.avaiableTables = data[1];
      })
  }

  onTableAssigned(eventId?: number, reservationId?: number, event?:any) {
    var tableId = event.target.value;
    var data: AssignTablePost = { tableId: tableId, eventId: eventId, reservationId: reservationId };

    this.api.assignTable(data).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe(data => {
        this.loadReservation();
      })
  }

  showTableMap() {
    this.api.getTableMap<TableMapFileInfo>().pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((url: any) => {
        open("http://127.0.0.1:8887/" + url.fileName, "", "width=800,height=600");
      })
  }
}
