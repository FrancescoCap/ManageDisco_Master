import { Directive } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from "@angular/forms";

@Directive({
  selector: '[emailValidator]',
  providers: [{
    provide: NG_VALIDATORS,
    useExisting: EmailValidatorCustom,
    multi: true    
  }]
})
export class EmailValidatorCustom implements Validator {


  validate(control: AbstractControl): ValidationErrors | null {
    if (control != null) {
      if (!control.value.includes("@") || !this.emailHasDomain(control.value)) {
        return { "invalidEmail": "Formato della email non valido." }
      } else {
        return null;
      }
    }
    else
      return { "invalidEmail": "Email mancante." };
  }

  private emailHasDomain(value:any) {
    var domain = value.split("@");
    return domain[1].includes(".");
  }

}
