import { Component, OnInit } from '@angular/core';
import { Route, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { ApiCaller } from '../../api/api';
import { client_URL } from '../../app.module';
import { AuthResponse, LoginRequest } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  loginRequest: LoginRequest = { email: "", password: "" };

  constructor(private _api: ApiCaller,
    private _router: Router,
    private _modal: ModalService  ) { }

  ngOnInit(): void {
  }

  public login() {
    this._api.login(this.loginRequest).pipe(
      catchError(err => {
        this._modal.showErrorOrMessageModal(err.message, "ERRORE");
        return err;
      })).subscribe((data: AuthResponse) => {       
        document.location.href = `${client_URL}/Home`;
    });
  }

}
