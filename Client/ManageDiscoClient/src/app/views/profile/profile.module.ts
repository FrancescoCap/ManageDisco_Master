import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ProfileComponent } from "./profile.component";
import { ProfileRoutes } from "./profile.rouote";
import { CommonModule } from "@angular/common";
import { LoadingModule } from "../../components/loading/loading.module";
import { DirectivesModule } from "../../directives/directive.module";

@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    LoadingModule,   
    ProfileRoutes,
    DirectivesModule
  ],
  declarations: [
    ProfileComponent
  ]
})
export class ProfileModule {}
