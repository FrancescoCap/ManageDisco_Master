import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { TableviewComponent } from "./tableview.component";

@NgModule({
  imports: [
    FormsModule,
    CommonModule
  ],
  declarations: [TableviewComponent],
  exports: [TableviewComponent]
})
export class TableViewModule {

}
