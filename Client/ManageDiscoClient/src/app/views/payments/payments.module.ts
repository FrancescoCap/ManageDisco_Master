import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { ModalModule } from "../../components/modal/modal.module";
import { PaymentsComponent } from "./payments.component";
import { PaymentsRoutes } from "./payments.route";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    LoadingModule,
    ModalModule,
    PaymentsRoutes
  ],
  declarations: [PaymentsComponent]
})
export class PaymentsModule {}
