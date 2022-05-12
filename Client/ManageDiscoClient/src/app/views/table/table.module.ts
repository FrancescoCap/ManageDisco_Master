import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { TableViewModule } from "../../components/tableview/table.module";
import { TableComponent } from "./table.component";
import { TableRoutes } from "./table.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    TableViewModule,
    TableRoutes
  ], declarations: [TableComponent]
})
export class TableModule {}
