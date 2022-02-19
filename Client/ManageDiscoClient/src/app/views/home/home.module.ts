import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { CarouselModule } from "../../components/carousel/carousel.module";
import { HomeComponent } from "./home.component";
import { HomeRouting } from "./home.routing";
import { SwiperModule } from "swiper/angular";
import { LoadingModule } from "../../components/loading/loading.module";

@NgModule({
  imports: [
    CommonModule,
    SwiperModule,
    LoadingModule,
    HomeRouting
  ],
  declarations: [HomeComponent]  
})
export class HomeModule {

}
