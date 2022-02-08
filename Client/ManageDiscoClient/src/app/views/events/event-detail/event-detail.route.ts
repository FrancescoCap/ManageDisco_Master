import { RouterModule } from "@angular/router";
import { EventDetailComponent } from "./event-detail.component";

export const EventDetailsRoute = RouterModule.forChild([
  {path:'', component: EventDetailComponent}
])
