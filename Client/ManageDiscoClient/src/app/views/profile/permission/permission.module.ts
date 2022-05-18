import { NgModule } from "@angular/core";
import { LoadingModule } from "../../../components/loading/loading.module";
import { TableViewModule } from "../../../components/tableview/table.module";
import { PermissionComponent } from "./permission.component";

@NgModule({
  imports: [
    TableViewModule,
    LoadingModule
  ],
  declarations: [PermissionComponent]
})
export class PermissionModule {}
