import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeSettingsRoute } from './home-settings.route';
import { FormsModule } from '@angular/forms';
import { HomeSettingsComponent } from './home-settings.component';
import { DropdownValidator } from '../../directives/drodownValidator.directive';
import { LoadingModule } from '../../components/loading/loading.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    LoadingModule,
    HomeSettingsRoute
  ],
  declarations: [
    HomeSettingsComponent,    
    DropdownValidator
  ]
  
})
export class HomeSettingsModule { }
