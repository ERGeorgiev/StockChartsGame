import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FlexModule } from '@angular/flex-layout';
import { } from '@angular/material';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { GameService } from './api/game.service';
import { IndicatorService } from './api/indicator.service';
import { VoteService } from './api/vote.service';
import { AppComponent } from './app.component';
import { ChartModule } from './chart/chart.module';
import { StyleService } from './style.service';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    // Angular
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule,
    FlexModule,
    ToastrModule.forRoot(),

    // Materials Design
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
    MatTooltipModule,
    MatCardModule,

    // Application
    ChartModule
  ],
  providers: [
    StyleService,
    GameService,
    IndicatorService,
    VoteService,
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
