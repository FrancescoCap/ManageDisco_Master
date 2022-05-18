import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { TableViewModule } from "../../../components/tableview/table.module";
import { ProfileUserreservationComponent } from "./profile-userreservation.component";

@NgModule({
  imports: [
    TableViewModule,
    CommonModule
  ],
  exports: [ProfileUserreservationComponent],
  declarations: [ProfileUserreservationComponent]
})
export class ProfileUserReservationModule {}
