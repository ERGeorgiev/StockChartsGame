import { Injectable } from '@angular/core';
import { FinancialDataPoint } from 'chart.js';
import { ToastrService } from 'ngx-toastr';
import * as rsx from 'rxjs';
import { GameService } from './game.service';
import { ChartService } from '../chart/chart.service';

@Injectable()
export class VoteService {
  hiddenPrices = 15;
  closeBeforeReveal = 0;
  votes = 0;
  votesCorrect = 0;
  value = '';
  voted = false;

  constructor(
    private cs: ChartService,
    private toastr: ToastrService,
    private game: GameService,
  ) {
    window.addEventListener('keydown', (e) => { // arrow function
      this.keyPress(e);
    })
  }

  keyPress(event: KeyboardEvent) {
    switch (event.keyCode) {
      case 37:
        break;
      case 38:
        this.up();
        break;
      case 39:
      case 13:
        this.reset();
        break;
      case 40:
        this.down();
        break;
    }
  }

  async reset() {
    if (this.voted) {
      console.log(`[VOTE] ------------------- Refresh`);
      this.cs.loading = true;
      await rsx.lastValueFrom(this.game.refreshQuotes());
      await this.cs.loadCharts();
      this.voted = false;
      let candlesticks = this.cs.chartOverlay.config.data.datasets[0].data;
      const close = (candlesticks[candlesticks.length - 1] as FinancialDataPoint).c;
      console.log(`[VOTE] Current Price: ${close}`);
    }
    else {
      let candlesticks = this.cs.chartOverlay.config.data.datasets[0].data;
      this.closeBeforeReveal = (candlesticks[candlesticks.length - 1] as FinancialDataPoint).c;
      console.log(`[VOTE] Close before reveal: ${this.closeBeforeReveal}`);
      console.log(`[VOTE] Candlesticks before reveal: ${candlesticks.length}`);
      await rsx.lastValueFrom(this.game.reveal());
      await this.cs.loadCharts(this.hiddenPrices);
      this.voted = true;
    }
  }

  up() {
    this.vote(true);
  }

  down() {
    this.vote(false);
  }

  async vote(vote: boolean) {
    if (this.voted === true) {
      return;
    }

    await this.reset();
    this.toastr.clear();

    let prices = this.cs.chartOverlay.config.data.datasets[0].data.map(d => (d as FinancialDataPoint).c);
    const changeRequired = 0.1;
    for (let p = prices.length - this.hiddenPrices; p < prices.length; p++) {
      const price = prices[p];
      let change = price - this.closeBeforeReveal;
      if (vote) {
        if (change > changeRequired) {
          this.votesCorrect++;
          this.votes++;
          this.toastr.success(`The price change was ${change.toFixed(2)}.`, `Correct! (${this.votesCorrect}/${this.votes})`);
          console.log(`[VOTE] Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
          return;
        }
        else if (change < -(changeRequired / 2)) {
          this.votes++;
          this.toastr.error(`The price change was ${change.toFixed(2)}.`, `Incorrect! (${this.votesCorrect}/${this.votes})`);
          console.log(`[VOTE] Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
          return;
        }
        else {
          console.log(`[VOTE] Not enough change: Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
        }
      }
      else {
        if (change < -changeRequired) {
          this.votesCorrect++;
          this.votes++;
          this.toastr.success(`The price change was ${change.toFixed(2)}.`, `Correct! (${this.votesCorrect}/${this.votes})`);
          console.log(`[VOTE] Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
          return;
        }
        else if (change > changeRequired / 2) {
          this.votes++;
          this.toastr.error(`The price change was ${change.toFixed(2)}.`, `Incorrect! (${this.votesCorrect}/${this.votes})`);
          console.log(`[VOTE] Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
          return;
        }
        else {
          console.log(`[VOTE] Not enough change: Price(${price}) - Close(${this.closeBeforeReveal}) = ${change.toFixed(2)}`);
        }
      }
    }

    this.toastr.error(`Not enough price change.`, 'Incorrect');
  }
}
