import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ModalModule } from "../../components/modal/modal.module";
import { ReservationComponent } from "./reservation.component";
import { reservationRoutes } from "./reservation.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ModalModule,
    reservationRoutes
  ],
  declarations: [ReservationComponent]
})
export class ReservationModule {}
