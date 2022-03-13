import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { LoadingModule } from "../../components/loading/loading.module";
import { CouponComponent } from "./coupon.component";
import { CouponRoute } from "./coupon.route";

@NgModule({
  imports: [
    CommonModule,
    LoadingModule,
    CouponRoute
  ], declarations: [CouponComponent]
})
export class CouponModule {}
