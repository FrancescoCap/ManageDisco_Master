import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ModalModule } from "../../components/modal/modal.module";
import { ModalService } from "../../service/modal.service";
import { TableOrderComponent } from "./table_order.component";
import { TableOrderRoute } from "./table_order.route";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    TableOrderRoute
  ], declarations: [TableOrderComponent]
})
export class TableOrderModule { }
