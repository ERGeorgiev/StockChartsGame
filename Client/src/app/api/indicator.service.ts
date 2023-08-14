import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
    ScatterDataPoint
} from 'chart.js';
import { Guid } from 'guid-typescript';
import { Observable } from 'rxjs';
import { env } from '../../environments/environment';
import {
    Indicator
} from '../chart/chart.models';

@Injectable()
export class IndicatorService {
  standardBlue = "#1E88E5";
  thresholdRed = "#B71C1C70";
  thresholdGreen = "#1B5E2070";
  extraBars = 7;

  constructor(private readonly http: HttpClient) { }

  getIndicatorEma(): Indicator {
    return {
      ucid: this.getGuid("chart"),
      label: `EMA([P1])`,
      id: `ema`,
      endpoint: `${env.api}/EMA/`,
      chartType: `overlay`,
      chartConfig: null,
      lookbackPeriod: 50,
      mainColor: this.standardBlue,
      dataset: {
        data: [],
        type: 'line',
        yAxisID: 'yAxis',
        pointRadius: 0,
        borderWidth: 2,
        borderColor: this.standardBlue,
        fill: false,
        order: 1
      }
    };
  }

  getIndicatorRsi(): Indicator {
    return {
      ucid: this.getGuid("chart"),
      label: `RSI([P1])`,
      id: `rsi`,
      endpoint: `${env.api}/RSI/`,
      chartType: `oscillator`,
      chartConfig: {
        minimumYAxis: 0,
        maximumYAxis: 100,
        thresholds: [
          {
            value: 70,
            color: this.thresholdRed,
            style: `dash`,
            fill: {
              target: `+2`,
              colorAbove: `transparent`,
              colorBelow: this.thresholdGreen
            }
          },
          {
            value: 30,
            color: this.thresholdGreen,
            style: `dash`,
            fill: {
              target: `+1`,
              colorAbove: this.thresholdRed,
              colorBelow: `transparent`
            }
          }

        ]
      },
      lookbackPeriod: 15,
      mainColor: this.standardBlue,
      dataset: {
        data: [],
        type: 'line',
        yAxisID: 'yAxis',
        pointRadius: 0,
        borderWidth: 2,
        borderColor: this.standardBlue,
        fill: false,
        order: 1
      }
    };
  }

  getGuid(prefix: string = "chart"): string {
    return `${prefix}${Guid.create().toString().replace(/-/gi, "")}`;
  }

  populateDataset(listing: Indicator): Observable<any> {
    const obs = new Observable((observer) => {
      // compose url
      let url = `${listing.endpoint}?&`;
      url += `lookbackPeriods=${listing.lookbackPeriod}`;

      // fetch data
      this.http.get(url, this.requestHeader())
        .subscribe({

          next: (apidata: any[]) => {
            // initialize dataset
            const data: ScatterDataPoint[] = [];
            const pointColor: string[] = [];
            const pointRotation: number[] = [];

            // populate data
            apidata.forEach(row => {

              let yValue = row[listing.id];

              // apply candle pointers
              pointColor.push(listing.mainColor);
              pointRotation.push(0);

              data.push({
                x: new Date(row.date).valueOf(),
                y: yValue
              });
            });

            // add extra bars
            const nextDate = new Date(Math.max.apply(null, data.map(h => new Date(h.x))));

            for (let i = 1; i < this.extraBars; i++) {
              nextDate.setDate(nextDate.getDate() + 1);
              data.push({
                x: new Date(nextDate).valueOf(),
                y: null
              });
            }

            listing.dataset.data = data;

            observer.next(listing);
          },

          error: (e: HttpErrorResponse) => {
            console.log("DATA", e);
            observer.error(e);
          }
        });

    });

    return obs;
  }

  requestHeader(): { headers?: HttpHeaders } {

    const simpleHeaders = new HttpHeaders()
      .set('Content-Type', 'application/json');

    return { headers: simpleHeaders };
  }
}
