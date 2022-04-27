import { Attribute } from "@angular/core";
import { Directive } from "@angular/core";
import { AbstractControl, FormControl, NG_VALIDATORS, ValidationErrors, Validator } from "@angular/forms";

@Directive({
  selector: '[passwordValidator]',
  providers: [{
    provide: NG_VALIDATORS,
    useExisting: PasswordValidator,
    multi: true
  }]
})
export class PasswordValidator implements Validator {

  upperCaseLetters: string[] = [
     "A",
     "B",
     "C",
     "D",
     "E",
     "F",
     "G",
     "H",
     "I",
     "L",
     "M",
     "N",
     "O",
     "P",
     "Q",
     "R",
     "S",
     "T",
     "U",
     "V",
     "W",
     "Y",
     "J",
     "K",
     "X",
  ] 

  specialChars: string[] = [
    "@",
    "!",
    "$",
    "%",
    "&",
    "#"
  ]
  numbers: string[] = [
    "1",
    "2",
    "3",
    "4",
    "5",
    "6",
    "7",
    "8",
    "9",
  ];

  constructor(@Attribute('compare-password') public comparer: string,
    @Attribute('parent') public parent: string) {
    
  }

  validate(control: AbstractControl): ValidationErrors | null {
    const compare_password = control.root.get(this.comparer);
    const password = control.value;    

    if (control.value == null)
      return { 'passwordValue': true };

    if (this.isParent && (!this.passwordContainsOneCapsLetter(password) ||
      !this.passwordContainsSpecialCharacters(password) ||
      !this.passwordHasRightLength(password)) ||
      !this.passwordHasNumber(password))
      return { 'passwordFormat': true };    
   
    if (!this.passwordMatch(password, compare_password?.value)) {
      console.log("mismatch")
      return { 'passwordMismatch': true };
    }
    
    return null;
  }

  public get isParent() {
    if (this.parent == null)
      return false;

    return this.parent === 'true';
  }

  passwordHasNumber(value:string) {
    var hasNumber = false;
    for (var i = 0; i < value.length; i++) {
      hasNumber = this.numbers.includes(value.charAt(i));
      if (hasNumber)
        break;
    }
    return hasNumber;
  }

  passwordMatch(password: string, password_compare: string) {
    //console.log(password == null)
    //console.log(password_compare == null)
    //console.log(password != password_compare)

    if (password == null)
      return false;
    if (password_compare == null)
      return false;
    if (password != password_compare)
      return false;

    return true;
  }

  passwordHasRightLength(value: any): boolean {
    return value.length >= 7;
  }

  passwordContainsOneCapsLetter(value: string):boolean {
    var upperCaseExist = false;
    for (var i = 0; i < value.length; i++) {
      upperCaseExist = this.upperCaseLetters.includes(value.charAt(i));
      if (upperCaseExist)
        break;
    }

    return upperCaseExist;
  }

  passwordContainsSpecialCharacters(value: any): boolean {
    var specialCharExist = false;
    
    for (var i = 0; i < value.length; i++) {
      specialCharExist = this.specialChars.includes(value.charAt(i));
      if (specialCharExist)
        break;
    }
    return specialCharExist;
  }



}
