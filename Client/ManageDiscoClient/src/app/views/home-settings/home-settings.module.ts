import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeSettingsRoute } from './home-settings.route';
import { FormsModule } from '@angular/forms';
import { HomeSettingsComponent } from './home-settings.component';
import { LoadingModule } from '../../components/loading/loading.module';
import { DirectivesModule } from '../../directives/directive.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    HomeSettingsRoute,
    DirectivesModule
  ],
  declarations: [
    HomeSettingsComponent
  ]
  
})
export class HomeSettingsModule { }
