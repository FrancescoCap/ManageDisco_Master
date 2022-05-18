import { Output } from '@angular/core';
import { Input } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.scss']
})
export class LoadingComponent implements OnInit {

  @Input("loading") loading?: boolean;
  @Input("loadingListener") loadingSubject?: Subject<boolean>;

  constructor() { }

  ngOnInit(): void {

  }

  ngAfterViewInit() {
    this.loadingSubject?.subscribe((visible: boolean) => this.loading = visible);
  }

}
