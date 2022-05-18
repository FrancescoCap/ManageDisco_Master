import { NgModule } from "@angular/core";
import { LoadingModule } from "../../../components/loading/loading.module";
import { TableViewModule } from "../../../components/tableview/table.module";
import { UserInfoComponent } from "./user-info.component";

@NgModule({
  imports: [
    TableViewModule,
    LoadingModule
  ], declarations:[UserInfoComponent]
})
export class UserInfoModule {}
