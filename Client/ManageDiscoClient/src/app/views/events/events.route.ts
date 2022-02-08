import { RouterModule } from "@angular/router";
import { EventDetailComponent } from "./event-detail/event-detail.component";
import { EventsComponent } from "./events.component";

export const eventRoutes = RouterModule.forChild([
  { path: '', component: EventsComponent },
  {path: 'Details', component: EventDetailComponent}
])
