import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RegistrationComponent } from "./registration.component";
import { RegistrationRoutes } from "./registration.route";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RegistrationRoutes
  ], declarations: [RegistrationComponent]
})
export class RegistrationModule {}
