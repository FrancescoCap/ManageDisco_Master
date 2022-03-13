import { NgModule } from "@angular/core";
import { DropdownValidator } from "./drodownValidator.directive";
import { EmailValidatorCustom } from "./emailValidator.directive";
import { PasswordValidator } from "./passwordValidator.directive";

@NgModule({
  imports:[],
  declarations: [
    DropdownValidator,
    EmailValidatorCustom,
    PasswordValidator
  ],
  exports: [    DropdownValidator,
    EmailValidatorCustom,
    PasswordValidator
  ]
})
export class DirectivesModule {}
