import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ModalModelList } from "../../components/modal/modal.model";
import { ModalModule } from "../../components/modal/modal.module";
import { ProductComponent } from "./product.component";
import { ProductsRoutes } from "./product.routes";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ModalModule,
    ProductsRoutes
  ],declarations:[ProductComponent]
})
export class ProductModule {}
