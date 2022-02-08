import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { eventRoutes } from './events.route';
import { EventsComponent } from './events.component';
import { ModalModule } from '../../components/modal/modal.module';
import { FormsModule } from '@angular/forms';



@NgModule({ 
  imports: [
    CommonModule,
    ModalModule,
    FormsModule,
    eventRoutes
  ],
  declarations: [EventsComponent]
})
export class EventsModule { }
