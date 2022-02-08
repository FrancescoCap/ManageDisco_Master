import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError } from 'rxjs';
import { ApiCaller } from '../../api/api';
import { RegistrationRequest } from '../../model/models';
import { ModalService } from '../../service/modal.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss', '../login/login.component.scss']
})
export class RegistrationComponent implements OnInit {

  registrationRequest: RegistrationRequest = { email: "", password: "", username: "", surname: "", name: "", prCode: ""};
  isCustomerRegistration = true;

  constructor(private _api: ApiCaller,
    private _modal: ModalService,
    private _router: Router,
    private _route: ActivatedRoute  ) { }

  ngOnInit(): void {
    this._route.queryParams.subscribe(params => {
      if (params["type"] != null)
        this.isCustomerRegistration = true;
    })
  }

  register() {
    this._api.register(this.registrationRequest).pipe(
      catchError(err => {
        this._modal.showErrorModal(err);
        return err;
      })).subscribe(() => {
        this._router.navigateByUrl("/Login");
      })
  }
}
