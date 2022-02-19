import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { TableComponent } from "./table.component";
import { TableRoutes } from "./table.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    TableRoutes
  ], declarations: [TableComponent]
})
export class TableModule {}
