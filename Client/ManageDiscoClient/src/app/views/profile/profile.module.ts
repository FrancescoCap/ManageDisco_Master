import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ProfileComponent } from "./profile.component";
import { ProfileRoutes } from "./profile.rouote";
import { CommonModule } from "@angular/common";
import { LoadingModule } from "../../components/loading/loading.module";
import { DirectivesModule } from "../../directives/directive.module";
import { TableViewModule } from "../../components/tableview/table.module";
import { ProfileUserreservationComponent } from './profile-userreservation/profile-userreservation.component';
import { StatisticsComponent } from "./statistics/statistics.component";
import { PermissionComponent } from './permission/permission.component';
import { UserInfoComponent } from './user-info/user-info.component';

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    LoadingModule,   
    ProfileRoutes,
    TableViewModule,
    DirectivesModule
  ],
  declarations: [
    ProfileComponent,
    ProfileUserreservationComponent,
    StatisticsComponent,
    PermissionComponent,
    UserInfoComponent
  ]
})
export class ProfileModule {}
