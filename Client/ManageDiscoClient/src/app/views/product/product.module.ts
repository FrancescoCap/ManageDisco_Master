import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { ModalModule } from "../../components/modal/modal.module";
import { ProductComponent } from "./product.component";
import { ProductsRoutes } from "./product.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    ModalModule,
    ProductsRoutes
  ],declarations:[ProductComponent]
})
export class ProductModule {}
