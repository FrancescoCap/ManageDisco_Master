import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeSettingsRoute } from './home-settings.route';
import { FormsModule } from '@angular/forms';
import { HomeSettingsComponent } from './home-settings.component';

@NgModule({
  declarations: [HomeSettingsComponent],
  imports: [
    CommonModule,
    FormsModule, 
    HomeSettingsRoute
  ]
})
export class HomeSettingsModule { }
