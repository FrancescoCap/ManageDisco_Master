import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { LoginComponent } from "./login.component";
import { LoginRouting } from "./login.routes";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    RouterModule,
    LoginRouting
  ],
  declarations:[LoginComponent]

})
export class LoginModule {}
