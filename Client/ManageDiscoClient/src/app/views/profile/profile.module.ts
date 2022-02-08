import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ProfileComponent } from "./profile.component";
import { ProfileRoutes } from "./profile.rouote";
import { ProfileReservationComponent } from './profile-reservation/profile-reservation.component';
import { CommonModule } from "@angular/common";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    ProfileRoutes
  ],
  declarations: [
    ProfileComponent
  ]
})
export class ProfileModule {}
