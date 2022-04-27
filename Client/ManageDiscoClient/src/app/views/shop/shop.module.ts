import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { ShopComponent } from "./shop.component";
import { ShopRoutes } from "./shop.route";

@NgModule({
  imports: [
    FormsModule,
    LoadingModule,
    CommonModule,
    ShopRoutes
  ], declarations:[ShopComponent]
})
export class ShopModule {}
