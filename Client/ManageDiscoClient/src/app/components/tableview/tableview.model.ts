import { EventEmitter } from "@angular/core";

export interface TableViewDataModel {
  isPdfExportable?: boolean;
  onExportPdfCallback?: Function;
  headers: TableViewHeader[];
  rows: TableViewRow[];
  onDataListChange?: EventEmitter<any>;
}

export interface TableViewHeader {
  value?: string;
}

export interface TableViewRow {
  id?: string;
  cells?: TableViewCell[];
  icon?: string;
}

export interface TableViewCell {
  value?: any;
  isCurrency?: boolean;
  isDropdown?: boolean;
  icon?: TableViewCellIcon[];
}

export interface TableViewCellIcon {
  class?: string;
  referenceId?: string; //item id for reference click
  isToShow?: boolean;
  isImage?: boolean;
  imagePath?: string;
  onClickCallback?: Function;
}

//export interface TableViewCellDropdown {
//  displayValue: string;
//  value: any;
//  list: any[];
//  onChangeCallback?: Function;
//}
