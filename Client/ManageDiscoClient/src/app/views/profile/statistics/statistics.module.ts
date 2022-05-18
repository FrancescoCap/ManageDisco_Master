import { NgModule } from "@angular/core";
import { TableViewModule } from "../../../components/tableview/table.module";
import { StatisticsComponent } from "./statistics.component";

@NgModule({
  imports: [
    TableViewModule
  ],
  declarations: [
    StatisticsComponent
  ],
  exports:[StatisticsComponent]
})
export class StatisticsModule {}
