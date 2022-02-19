import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { RegistrationComponent } from "./registration.component";
import { RegistrationRoutes } from "./registration.route";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    RegistrationRoutes
  ], declarations: [RegistrationComponent]
})
export class RegistrationModule {}
