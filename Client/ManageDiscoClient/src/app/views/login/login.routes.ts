import { RouterModule } from "@angular/router";
import { LoginComponent } from "./login.component";

export const LoginRouting = RouterModule.forChild([
  {path:'', component: LoginComponent}
])
