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

}
