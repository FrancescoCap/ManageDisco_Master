import { ModalModelEnum, ModalViewGroup } from "../components/modal/modal.model";

export class GeneralMethods {

  public static normalizeBase64(value: string) {
    return `data:image/webp;base64,${value}`;
  }

  public static getLoginModalViews():ModalViewGroup[] {
    var modalViews: ModalViewGroup[] = [
      {
        type: ModalModelEnum.TextBox, viewItems: [
          { viewId: "txtEmail", label: "Email", referenceId: "email" },
          { viewId: "txtPassword", label: "Password", referenceId: "password" }]
      }
    ];
    return modalViews;
  }
  /**
   * 
   * @param args default text for reservation, Titles below are used to map referenceId also:
   * 1. reservationName
   * 2. peopleCount
   * 3. expectedBudget
   * 4. realBudget
   */
  public static getEditReservationModalViews(...args:any[]): ModalViewGroup[] {
    return [{
      type: ModalModelEnum.TextBox, viewItems: [
        { viewId: "txtReservationName", referenceId: "reservationName", defaultText: args[0], label: "Nome prenotazione"},
        { viewId: "txtPeopleCount", referenceId: "peopleCount", defaultText: args[1], label: "Nr. persone"},
        { viewId: "txtExpectedBudget", referenceId: "expectedBudget", defaultText: args[2], label: "Budget previsto"},
        { viewId: "txtRealBudget", referenceId: "realBudget", defaultText: args[3], label: "Budget reale"},
      ]
    }];
  }

  /**
   * 
   * @param args default text for assignable tables, Titles below are used to map referenceId also:
   * 1. tableId (passing assignable tables list)
   */
  public static getAssignTableModalViews(...args: any[]): ModalViewGroup[] {
    return [{
      type: ModalModelEnum.Dropdown, viewItems: [
        { viewId: "drpTableId", referenceId: "tableId", label: "Posizione tavolo", list: args[0] }
      ]
    }];
  }

}
