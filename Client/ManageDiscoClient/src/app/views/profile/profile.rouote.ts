import { RouterModule } from "@angular/router";
import { ProfileReservationComponent } from "./profile-reservation/profile-reservation.component";
import { ProfileComponent } from "./profile.component";

export const ProfileRoutes = RouterModule.forChild([
  { path: '', component: ProfileComponent },
  { path:'Reservation', component: ProfileReservationComponent}

])
