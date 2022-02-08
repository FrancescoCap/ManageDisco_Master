import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { last } from "rxjs";
import { EventDetailComponent } from "./event-detail.component";
import { EventDetailsRoute } from "./event-detail.route";
import { SwiperModule } from "swiper/angular";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    SwiperModule,
    BrowserModule,   
    EventDetailsRoute
  ], declarations: [EventDetailComponent]

})
export class EventDetailModule {}
