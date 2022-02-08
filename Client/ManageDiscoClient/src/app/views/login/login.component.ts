import { Component, OnInit } from '@angular/core';
import { Route, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { AuthResponse, LoginRequest } from '../../model/models';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  loginRequest: LoginRequest = { email: "", password: "" };

  constructor(private _api: ApiCaller,
              private _router:Router  ) { }

  ngOnInit(): void {
  }

  public login() {
    this._api.login(this.loginRequest).pipe(
      catchError(err => {
        console.log(err);
        return err;
      })).subscribe((data: AuthResponse) => {
        if (data.token != null) {
          document.cookie = "auth_consent=" + data.token;          
          document.location.href = "http://localhost:4200/Home"         
        }          
    });
  }

}
