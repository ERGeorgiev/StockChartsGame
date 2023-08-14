import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
    ChartDataset,
    FinancialDataPoint, ScatterDataPoint,
    Tick
} from 'chart.js';
import Chart from 'chart.js/auto';
import 'chartjs-adapter-date-fns';
import 'chartjs-chart-financial';
import {
    CandlestickController,
    CandlestickElement,
    OhlcController,
    OhlcElement
} from 'chartjs-chart-financial';
import annotationPlugin, { AnnotationOptions, ScaleValue } from 'chartjs-plugin-annotation';
import * as rsx from 'rxjs';
import { GameService } from '../api/game.service';
import { IndicatorService } from '../api/indicator.service';
import { StyleService } from '../style.service';
import { ChartBaseService } from './chart-base.service';
import {
    ChartThreshold,
    Indicator,
    Quote
} from './chart.models';

Chart.register(
  CandlestickController,
  OhlcController,
  CandlestickElement,
  OhlcElement,
  annotationPlugin);

@Injectable()
export class ChartService extends ChartBaseService {

  constructor(
    private readonly game: GameService,
    private readonly indicatorService: IndicatorService,
    readonly ts: StyleService
  ) {
    super(ts);
    this.indicators.push(this.indicatorService.getIndicatorEma());
    this.indicators.push(this.indicatorService.getIndicatorRsi());
  }

  yAxisTicks: Tick[] = [];
  indicators: Indicator[] = [];
  chartOverlay: Chart;
  loading = true;

  addListingWithoutScroll(
    listing: Indicator
  ) {
    this.indicatorService.populateDataset(listing)
      .subscribe({
        next: (listingWithData: Indicator) => {
          this.displayListing(listingWithData, false);
        },
        error: (e: HttpErrorResponse) => { console.log(e); }
      });
  }

  displayListing(
    listing: Indicator,
    scrollToMe: boolean
  ) {
    if (listing.chartType == 'overlay') {
      this.addListingToOverlayChart(listing, scrollToMe);
    }
    else {
      this.addSelectionToNewChart(listing, scrollToMe);
    };

  }

  addListingToOverlayChart(
    listing: Indicator,
    scrollToMe: boolean) {

    this.chartOverlay.data.datasets.push(listing.dataset);
    this.chartOverlay.update(); // ensures scales are drawn to correct size first
    this.updateOverlayAnnotations();
    this.chartOverlay.update();

    if (scrollToMe) this.scrollToStart("chart-overlay");
  }

  addSelectionToNewChart(
    listing: Indicator,
    scrollToMe: boolean) {
    const chartConfig = this.baseOscillatorConfig();
    chartConfig.data = {
      datasets: []
    };

    // add thresholds (reference lines)
    const qtyThresholds = listing.chartConfig?.thresholds?.length;

    listing.chartConfig?.thresholds?.forEach((threshold: ChartThreshold, index: number) => {

      const lineData: ScatterDataPoint[] = [];

      // compose threshold data
      listing.dataset.data.forEach((d: ScatterDataPoint) => {
        lineData.push({ x: d.x, y: threshold.value } as ScatterDataPoint);
      });

      const thresholdDataset: ChartDataset = {
        label: "threshold",
        type: 'line',
        data: lineData,
        yAxisID: 'yAxis',
        pointRadius: 0,
        borderWidth: 2.5,
        borderDash: threshold.style == "dash" ? [5, 2] : [],
        borderColor: threshold.color,
        backgroundColor: threshold.color,
        spanGaps: true,
        fill: threshold.fill == null ? false : {
          target: threshold.fill.target,
          above: threshold.fill.colorAbove,
          below: threshold.fill.colorBelow
        },
        order: index + 100
      };

      chartConfig.data.datasets.push(thresholdDataset);
    });

    // hide thresholds from tooltips
    if (qtyThresholds > 0) {
      chartConfig.options.plugins.tooltip.filter = (tooltipItem) =>
        (tooltipItem.datasetIndex > (qtyThresholds - 1));
    }

    // y-scale
    chartConfig.options.scales.yAxis.min = listing.chartConfig?.minimumYAxis;
    chartConfig.options.scales.yAxis.max = listing.chartConfig?.maximumYAxis;

    // add selection
    chartConfig.data.datasets.push(listing.dataset);

    // compose html
    const body = document.getElementById("oscillators-zone");
    const containerId = `${listing.ucid}-container`;

    // pre-delete, if exists (needed for theme change)
    const existing = document.getElementById(containerId);
    if (existing != null) {
      body.removeChild(existing);
    }

    // create chart container
    const container = document.createElement('div') as HTMLDivElement;
    container.id = containerId;
    container.className = "chart-oscillator-container";

    // add chart
    const myCanvas = document.createElement('canvas') as HTMLCanvasElement;
    myCanvas.id = listing.ucid;
    container.appendChild(myCanvas);
    body.appendChild(container);

    if (listing.chart) listing.chart.destroy();
    listing.chart = new Chart(myCanvas.getContext("2d"), chartConfig);

    // annotations
    const xPos: ScaleValue = listing.chart.scales["xAxis"].getMinMax(false).min;
    const yPos: ScaleValue = listing.chart.scales["yAxis"].getMinMax(false).max;

    const labelColor = this.ts.isDarkTheme ? '#757575' : '#212121';
    const annotation: AnnotationOptions =
      this.commonAnnotation(listing.label, labelColor, xPos, yPos, -2, 1);
    listing.chart.options.plugins.annotation.annotations = { annotation };
    listing.chart.update();

    if (scrollToMe) this.scrollToEnd(container.id);
  }

  updateOverlayAnnotations() {

    const xPos: ScaleValue = this.chartOverlay.scales["xAxis"].getMinMax(false).min;
    const yPos: ScaleValue = this.chartOverlay.scales["yAxis"].getMinMax(false).max;
    let adjY: number = 0;

    this.chartOverlay.options.plugins.annotation.annotations =
      this.indicators
        .filter(x => x.chartType == 'overlay')
        .map((listing: Indicator, index: number) => {
          const annotation: AnnotationOptions =
            this.commonAnnotation(listing.label, listing.mainColor, xPos, yPos, -2, adjY);
          annotation.id = "legend" + (index + 1).toString();
          adjY += 12;
          return annotation;
        });
  }

  async loadCharts(animateLastN: number = 0) {
    console.log("[CHART] Beginning to load charts...");
    this.loading = true;
    const quotes = await rsx.lastValueFrom(this.game.getQuotes());
    this.loading = false;

    for (let i = animateLastN; i >= 0; i--) {
      const animQuotes = quotes.slice(0, quotes.length - i);
      this.loadOverlayChart(animQuotes);
      await new Promise(f => setTimeout(f, 25));
    }

    // load default selections
    this.indicators.forEach(i => this.addListingWithoutScroll(i));
    console.log("[CHART] Charts loaded.");
  }

  loadOverlayChart(quotes: Quote[]) {
    const chartConfig = this.baseOverlayConfig();
    const candleOptions = Chart.defaults.elements["candlestick"];

    // custom border colors
    candleOptions.color.up = '#1B5E20';
    candleOptions.color.down = '#B71C1C';
    candleOptions.color.unchanged = '#616161';

    candleOptions.borderColor = {
      up: candleOptions.color.up,
      down: candleOptions.color.down,
      unchanged: candleOptions.color.unchanged
    };

    let price: FinancialDataPoint[] = [];
    let volume: ScatterDataPoint[] = [];
    const barColor: string[] = [];

    let sumVol = 0;

    quotes.forEach((q: Quote) => {

      price.push({
        x: new Date(q.date).valueOf(),
        o: q.open,
        h: q.high,
        l: q.low,
        c: q.close
      });

      volume.push({
        x: new Date(q.date).valueOf(),
        y: q.volume
      });
      sumVol += q.volume;

      const c = (q.close >= q.open) ? '#333333' : '#333333';
      barColor.push(c);
    });

    // add extra bars
    const nextDate = new Date(Math.max.apply(null, quotes.map(h => new Date(h.date))));

    for (let i = 1; i < this.indicatorService.extraBars; i++) {
      nextDate.setDate(nextDate.getDate() + 1);

      // intentionally excluding price (gap covered by volume)
      volume.push({
        x: new Date(nextDate).valueOf(),
        y: null
      });
    }

    chartConfig.data = {
      datasets: [
        {
          type: 'candlestick',
          label: 'Price',
          data: price,
          yAxisID: 'yAxis',
          borderColor: candleOptions.borderColor,
          order: 75
        },
        {
          type: 'bar',
          label: 'Volume',
          data: volume,
          yAxisID: 'volumeAxis',
          backgroundColor: barColor,
          borderWidth: 0,
          order: 76
        }
      ]
    };

    // get size for volume axis
    const volumeAxisSize = 20 * (sumVol / volume.length) || 0;
    chartConfig.options.scales.volumeAxis.max = volumeAxisSize;

    // compose chart
    if (this.chartOverlay) this.chartOverlay.destroy();
    const myCanvas = document.getElementById("chartOverlay") as HTMLCanvasElement;
    this.chartOverlay = new Chart(myCanvas.getContext('2d'), chartConfig);
    const symbol = document.getElementById("symbol") as HTMLCanvasElement;
    this.game.getSymbol()
      .subscribe({
        next: (stock: string) => {
          symbol.textContent = stock;
        },
        error: (e: HttpErrorResponse) => { console.log(e); }
      });
  }
}
