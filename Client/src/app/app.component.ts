import { Component, OnInit } from '@angular/core';
import { ChartService } from './chart/chart.service';
import { StyleService } from './style.service';
import { VoteService } from './api/vote.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})

export class AppComponent implements OnInit {

  constructor(
    public chart: ChartService,
    public readonly ts: StyleService,
    public readonly vs: VoteService
  ) {
  }

  ngOnInit(): void {
    this.ts.getTheme();
  }
}
