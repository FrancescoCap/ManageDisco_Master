import { RouterModule } from "@angular/router";
import { ReservationComponent } from "../reservation/reservation.component";
import { HomeComponent } from "./home.component";

export const HomeRouting = RouterModule.forChild([
  {path: '', component: HomeComponent}
])
