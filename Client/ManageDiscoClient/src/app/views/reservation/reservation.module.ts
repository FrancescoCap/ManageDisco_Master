import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { ModalModule } from "../../components/modal/modal.module";
import { TableViewModule } from "../../components/tableview/table.module";
import { ReservationComponent } from "./reservation.component";
import { reservationRoutes } from "./reservation.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ModalModule,
    LoadingModule,
    TableViewModule,
    reservationRoutes
  ],
  declarations: [ReservationComponent]
})
export class ReservationModule {}
