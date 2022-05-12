import { Component, EventEmitter, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { server_URL } from '../../app.module';
import { TableViewDataModel } from '../../components/tableview/tableview.model';
import { GeneralMethods } from '../../helper/general';
import { AssignTablePost, EventParty, EventPartyView, Reservation, ReservationStatus, ReservationViewTable, Table, TableMapFileInfo } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss', '../../app.component.scss']
})
export class TableComponent implements OnInit {

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  selectedReservationId: number = -1;

  tableViewData: TableViewDataModel = {
    headers: [
      { value: "Nome tavolo"},
      { value: "Nr.persone previste"},
      { value: "Budget previsto"},
      { value: "Tavolo assegnato"},
      { value: ""},
    ], rows: []
  }

  acceptedReservations: Reservation[] = [];
  events?: EventPartyView;
  reservationStatus: ReservationStatus[] = [];
  avaiableTables: Table[] = [];

  selectedEventId: number = 0;
  selectedResStatusId: number = 0;
  isLoading = false;
  isMobileView = false;
  tableRowPage = 10;

  constructor(private api: ApiCaller,
    private _modal: ModalService,
    private _generalService:GeneralService  ) { }

  ngOnInit(): void {
    this.isMobileView = this._generalService.isMobileView();
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
      this.api.getTables()
    ]

    forkJoin(calls).pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: any) => {
        this.events = data[0];
        this.acceptedReservations = data[1];
        this.reservationStatus = data[2];
        this.avaiableTables = data[3];
        this.setDataForTableView();
        this.tableViewData.onDataListChange = new EventEmitter<any>();

        this.isLoading = false;
      })
  }

  setDataForTableView() {
    if (this.tableViewData.rows.length > 0)
      this.tableViewData.rows = [];    

    this.acceptedReservations?.forEach((x, y) => {
      this.tableViewData.rows.push({
        cells: [
          { value: x.reservationName },
          { value: x.reservationPeopleCount },
          { value: x.reservationExpectedBudget, isCurrency: true },          
          { value: x.reservationTablAssigned },
          {
            value: "", icon: [
              { class: "fa fa-map", referenceId: `map_${x.reservationId}`, isToShow: true, onClickCallback: this.onTableLocationClick }
            ]
          }
        ]
      })
    });    
  }

  onTableLocationClick = (data: any): void => {
    this.selectedReservationId = data;
    var assignTableModalViews = GeneralMethods.getAssignTableModalViews(this.avaiableTables);

    this._modal.showAddModal(this.onTableLoactionConfirmed, "ASSEGNA TAVOLO", assignTableModalViews)
  }

  onTableLoactionConfirmed = (data: any): void => {
    var dataPost: AssignTablePost = { tableId: data.get("tableId"), reservationId: this.selectedReservationId, eventId: this.selectedEventId };

    this.api.assignTable(dataPost).pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe(data => {
        this._modal.hideModal();
        console.log(data)
        if (data.message != null) 
          this._modal.showErrorOrMessageModal(data.message, "ESITO", true);

       
        this.loadReservation();
      })
  }

  onEventChange(event: any) {
    this.selectedEventId = event.target.value;    
    this.loadReservation();    
  }

  onResStatusChange(status: any) {
    this.selectedResStatusId = status.target.value;
    this.loadReservation();
  }

  loadReservation() {
    forkJoin([
      //different endpoints in getReservations()
      this.api.getReservations(this.selectedEventId, this.selectedResStatusId),
      this.api.getTables()
    ]).subscribe((data: any) => {      
      this.acceptedReservations = data[0];
      this.avaiableTables = data[1];
      this.setDataForTableView();
      this.tableViewData.onDataListChange?.emit(this.tableViewData);
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
        open(server_URL.replace("api/", "") + url.fileName, "", "width=800,height=600");
      })
  }
}
