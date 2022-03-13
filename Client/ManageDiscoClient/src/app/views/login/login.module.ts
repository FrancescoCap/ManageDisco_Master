import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { DirectivesModule } from "../../directives/directive.module";
import { LoginComponent } from "./login.component";
import { LoginRouting } from "./login.routes";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    RouterModule,
    LoginRouting,
    DirectivesModule
  ],
  declarations: [
    LoginComponent
  ]

})
export class LoginModule {}
