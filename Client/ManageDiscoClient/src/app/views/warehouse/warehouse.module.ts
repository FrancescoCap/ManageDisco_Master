import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { WarehouseComponent } from "./warehouse.component";
import { WarehouseRoute } from "./warehouse.route";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    WarehouseRoute
  ], declarations: [WarehouseComponent]
})
export class WarehouseModule {}
