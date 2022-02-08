export enum ModalModelEnum {
  TextBox,
  Dropdown,
  ImageBox,
  Table,
  Label,
  Textarea  
}

export interface ModalModelType {
  type?: ModalModelEnum;
  viewItems: ViewItem[];
}

/****************************
   - referenceId: nome della chiave nell'oggetto Map restiutito
   - viewId: id della view
   - list: lista dei dati da mostrare (in caso di dropdown)
   - mapList: per situazioni di necessità di selezione multipla (vedi ordine-bottiglie)
   - label: nome del campo richiesto
   - valueDisplay: nome del campo da visualizzare nel controllo (uso per dropdown)
****************************/
export interface ViewItem {
  viewId?: any,
  label?: string,
  referenceId?: any,
  valueDisplay?: any,
  list?: any[],
  mapList?: { [key: string]: number }
  hasNgModel?: boolean;
}

export class ModalModelList implements ModalModelType {
  viewItems: ViewItem[] = [];

  type?: ModalModelEnum;  dropId: any;
  label: string = "";
  id: any = "";
  valueDisplay: string = "";
  list: any[] = [];
  mapList?: { [key: number]: number }

}

export class ModalTextBoxList {
  id: string = "";
  label: string = "";
}

export class ModalImageBoxList  {

  type?: ModalModelEnum;
  id: string = "";
}


export class ModalViewGroup{
  viewItems: ViewItem[] = [];
  type: ModalModelEnum = ModalModelEnum.TextBox;  
}
