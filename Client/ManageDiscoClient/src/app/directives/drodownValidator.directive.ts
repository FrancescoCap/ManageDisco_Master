import { Directive } from "@angular/core";
import { AbstractControl,  NG_VALIDATORS, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";

@Directive({
  selector: '[dropdownValidator]',
  providers: [{
    provide: NG_VALIDATORS,
    useExisting: DropdownValidator,
    multi: true   
  }]
})
export class DropdownValidator implements Validator {

  validate(control: AbstractControl): ValidationErrors | null {
      return control.value == 0 ? { "invalidSelect": true } : null;
  } 


}
