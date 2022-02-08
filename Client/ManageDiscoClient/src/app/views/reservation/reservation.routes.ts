import { RouterModule } from "@angular/router";
import { ReservationComponent } from "./reservation.component";

export const reservationRoutes = RouterModule.forChild([
  {path: '', component: ReservationComponent}
])
