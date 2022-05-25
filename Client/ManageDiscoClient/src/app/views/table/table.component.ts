import { Component, EventEmitter, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { toArray } from 'rxjs';
import { concatMap } from 'rxjs';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { server_URL } from '../../app.module';
import { TableViewDataModel } from '../../components/tableview/tableview.model';
import { GeneralMethods } from '../../helper/general';
import { AssignTablePost, EventParty, EventPartiesViewInfo, Reservation, ReservationStatus, ReservationViewTable, Table, TableEvents, TableEventView, TableMapFileInfo } from '../../model/models';
import { GeneralService } from '../../service/general.service';
import { ModalService } from '../../service/modal.service';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss', '../../app.component.scss']
})
export class TableComponent implements OnInit {

  private readonly AUTOASSIGN_EVENT_ERROR_MESSAGE = "Per attivare l'autoassegnazione dei tavoli è necessario selezionare un evento.";

  @ViewChild("modalContainer", { read: ViewContainerRef, static: false }) modalContainer?: ViewContainerRef;

  selectedReservationId: number = -1;

  tableViewData: TableViewDataModel = {
    isPdfExportable: true,
    headers: [
      { value: "Nome tavolo"},
      { value: "Nr.persone previste"},
      { value: "Budget previsto"},
      { value: "Tavolo assegnato"},
      { value: ""},
    ], rows: []
  }

  acceptedReservations: Reservation[] = [];
  events: EventParty[] = [];
  reservationStatus: ReservationStatus[] = [];
  avaiableTables: Table[] = [];

  selectedEventId: number = 0;
  selectedResStatusId: number = 0;
  isLoading = false;
  isMobileView = false;
  tableRowPage = 10;
  userIsAdministrator = false;

  constructor(private api: ApiCaller,
    private _modal: ModalService,
    private _generalService: GeneralService,
    private _userService:UserService) { }

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
      this.api.getEvents(false),
      this.api.getReservations(0, 0),
      this.api.getReservationStatus(),
      this.api.getTables()
    ]

   this.api.getEventReservationTables().pipe(
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: TableEventView) => {
        this.events = data.eventParties!;
        this.acceptedReservations = data.reservations!;
        this.reservationStatus = data.reservationStatus!;
        this.avaiableTables = data.tables!;
        console.log(data.tables)
        console.log(this.avaiableTables)
        this.userIsAdministrator = data.userCanHandleReservation!;

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
              { class: "fa fa-map", referenceId: `map_${x.reservationId}`, isToShow: this.userIsAdministrator, onClickCallback: this.onTableLocationClick }
            ]
          }
        ]
      })
    });

    this.tableViewData.onExportPdfCallback = this.onPdfExport;
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
    this.api.getEventReservationTables(this.selectedEventId).subscribe((data: TableEventView) => {      
      this.acceptedReservations = data.reservations!;
      this.avaiableTables = data.tables!;
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

  autoAssignTable() {
    this.isLoading = true;
    if (this.selectedEventId <= 0) {
      this.isLoading = false;
      this._modal.showErrorOrMessageModal(this.AUTOASSIGN_EVENT_ERROR_MESSAGE, "ERRORE");
      return;
    }

    var calls = of(this.api.postAutoAssignTable(this.selectedEventId), this.api.getReservations(this.selectedEventId, this.selectedResStatusId));

    calls.pipe(
      concatMap((values: any) => {
        this.acceptedReservations = values;
        return values;
      }),
      toArray(),
      catchError(err => {
        this.isLoading = false;
        return err;
      })).subscribe((data: any) => {
        this.acceptedReservations = data[1];
        this.setDataForTableView();
        this.isLoading = false;
      })
     
  }
  //API call to get pdf of accepted reservations
  onPdfExport = (): any => {
   
    this.isLoading = true;

    this.api.exportTables(this.selectedEventId)
      .pipe(catchError (err => { this.isLoading = false; return err;}))
      .subscribe((file: Blob) => {
        
        var fileUrl = window.URL.createObjectURL(file);        
        window.open(fileUrl);
        this.isLoading = false;
      });
  }
}
