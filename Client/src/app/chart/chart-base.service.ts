import { Injectable } from '@angular/core';
import {
    ChartConfiguration, FontSpec,
    ScaleOptions, Tick
} from 'chart.js';
import Chart from 'chart.js/auto'; // import all default options
import 'chartjs-adapter-date-fns';
import 'chartjs-chart-financial';
// extensions
import {
    CandlestickController,
    CandlestickElement,
    OhlcController,
    OhlcElement
} from 'chartjs-chart-financial';
// plugins
import annotationPlugin, { AnnotationOptions, ScaleValue } from 'chartjs-plugin-annotation';
import { enUS } from 'date-fns/locale';
import { StyleService } from '../style.service';

Chart.register(
  CandlestickController,
  OhlcController,
  CandlestickElement,
  OhlcElement,
  annotationPlugin);

@Injectable()
export class ChartBaseService {

  constructor(
    readonly ts: StyleService
  ) { }

  yAxisTicks: Tick[] = [];

  baseConfig() {
    const commonXaxes = this.commonXAxes();
    const gridColor = this.ts.isDarkTheme ? '#424242' : '#CCCCCC';
    const config: ChartConfiguration = {
      type: 'candlestick',
      data: {
        datasets: []
      },
      plugins: [],
      options: {
        plugins: {
          title: {
            display: false
          },
          legend: {
            display: false
          },
          tooltip: {
            enabled: false,
            mode: 'index',
            intersect: false
          },
          annotation: {
            clip: false,
            drawTime: 'afterDraw',
            annotations: []
          }
        },
        layout: {
          padding: 0,
          autoPadding: false
        },
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        scales: {
          xAxis: commonXaxes,
          yAxis: {
            alignToPixels: true,
            display: true,
            type: 'linear',
            axis: 'y',
            position: 'right',
            beginAtZero: false,
            ticks: {
              display: true,
              mirror: true,
              padding: -5,
              font: {
                size: 12,
                lineHeight: 1
              },
              showLabelBackdrop: true,
              backdropColor: this.ts.isDarkTheme ? '#212121' : 'white',
              backdropPadding: {
                top: 0,
                left: 2,
                bottom: 0,
                right: 2
              },
            },
            grid: {
              drawOnChartArea: true,
              drawTicks: false,
              drawBorder: false,
              lineWidth: 0.5,
              color: function (context) {
                if (context.tick.label === null) {
                  return 'transparent';
                } else {
                  return gridColor;
                }
              },

            }
          }
        }
      }
    };

    return config;
  }

  baseOverlayConfig(): ChartConfiguration {

    const config = this.baseConfig();
    config.type = 'candlestick';

    // format y-axis, add dollar sign
    config.options.scales.yAxis.ticks.callback = (value, index, values) => {

      this.yAxisTicks = values;

      if (index === 0 || index === values.length - 1) return null;
      else
        return '$' + value;
    };

    // volume axis
    config.options.scales.volumeAxis = {
      display: false,
      type: 'linear',
      axis: 'y',
      position: 'left',
      beginAtZero: true
    } as ScaleOptions;

    return config;
  }

  baseOscillatorConfig(): ChartConfiguration {

    const config = this.baseConfig();

    // remove x-axis
    config.options.scales.xAxis.display = false;

    // top padding
    config.options.layout.padding = {
      top: 5,
      right: 0,
      bottom: 0,
      left: 0
    };

    // remove first and last y-axis labels
    config.options.scales.yAxis.ticks.callback = (value: number, index, values) => {

      this.yAxisTicks = values;

      if (index === 0 || index === values.length - 1) return null;
      else if (value > 10000000000) {
        return value / 1000000000 + "B";
      }
      else if (value > 10000000) {
        return value / 1000000 + "M";
      }
      else if (value > 10000)
        return value / 1000 + "K";
      else if (value < 10)
        return Math.round((value + Number.EPSILON) * 100000000) / 100000000;
      else
        return value;
    };

    return config;
  }

  commonXAxes(): ScaleOptions {

    const axes: ScaleOptions = {
      alignToPixels: true,
      display: true,
      type: 'timeseries',
      time: {
        unit: 'minute'
      },
      adapters: {
        date: {
          locale: enUS
        },
      },
      ticks: {
        source: "data",
        padding: 0,
        autoSkip: true,
        maxRotation: 0,
        minRotation: 0,
        font: {
          size: 12
        },
      },
      grid: {
        drawOnChartArea: true,
        drawBorder: true,
        tickLength: 1
      }
    };

    return axes;
  }

  commonAnnotation(
    label: string,
    fontColor: string,
    xPos: ScaleValue,
    yPos: ScaleValue,
    xAdj: number = 0,
    yAdj: number = 0
  ): AnnotationOptions {

    const legendFont: FontSpec = {
      family: "Roboto",
      size: 11,
      style: "normal",
      weight: "normal",
      lineHeight: 1,
    };

    const annotation: AnnotationOptions = {
      type: 'label',
      content: [label],
      font: legendFont,
      color: fontColor,
      backgroundColor: this.ts.isDarkTheme ? 'rgba(33,33,33,0.5)' : 'rgba(255,255,255,0.7)',
      padding: 0,
      position: 'start',
      xScaleID: 'xAxis',
      yScaleID: 'yAxis',
      xValue: xPos,
      yValue: yPos,
      xAdjust: xAdj,
      yAdjust: yAdj
    };

    return annotation;
  }

  scrollToStart(id: string) {
    setTimeout(() => {
      const element = document.getElementById(id);
      element.scrollIntoView({ behavior: 'smooth', block: 'start', inline: 'start' });
    }, 200);
  }

  scrollToEnd(id: string) {
    setTimeout(() => {
      const element = document.getElementById(id);
      element.scrollIntoView({ behavior: 'smooth', block: 'end', inline: 'end' });
    }, 200);
  }
}
