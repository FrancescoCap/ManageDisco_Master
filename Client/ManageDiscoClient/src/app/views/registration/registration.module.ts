import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LoadingModule } from "../../components/loading/loading.module";
import { DirectivesModule } from "../../directives/directive.module";
import { RegistrationComponent } from "./registration.component";
import { RegistrationRoutes } from "./registration.route";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    DirectivesModule,
    RegistrationRoutes
  ], declarations: [RegistrationComponent]
})
export class RegistrationModule {}
