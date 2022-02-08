import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { TableComponent } from "./table.component";
import { TableRoutes } from "./table.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    TableRoutes
  ], declarations: [TableComponent]
})
export class TableModule {}
