import { Component, OnInit } from '@angular/core';
import { ApiCaller } from '../../api/api';
import { User } from '../../model/models';

@Component({
  selector: 'app-collaborator',
  templateUrl: './collaborator.component.html',
  styleUrls: ['./collaborator.component.scss']
})
export class CollaboratorsComponent implements OnInit {

  user?: User;

  constructor(private _api: ApiCaller) { }

  ngOnInit(): void {
  }

  initData() {
    
  }

}
