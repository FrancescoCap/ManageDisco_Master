import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { TableOrderComponent } from "./table_order.component";
import { TableOrderRoute } from "./table_order.route";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    LoadingModule,
    TableOrderRoute
  ], declarations: [TableOrderComponent]
})
export class TableOrderModule { }
