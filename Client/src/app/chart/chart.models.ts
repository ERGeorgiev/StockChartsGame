import {
    Chart,
    ChartDataset
} from "chart.js";

export interface Quote {
  date: Date;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}

// LISTING

export interface Indicator {
  ucid: string,
  label: string;
  id: string;
  endpoint: string;
  chartType: string;
  mainColor: string;
  chartConfig: ChartConfig | null;
  lookbackPeriod: number;
  chart?: Chart;
  dataset?: ChartDataset;
}

export interface ChartConfig {
  minimumYAxis: number | null;
  maximumYAxis: number | null;
  thresholds: ChartThreshold[];
}

export interface ChartThreshold {
  value: number;
  color: string;
  style: string;
  fill: ChartFill;
}

export interface ChartFill {
  target: string,
  colorAbove: string,
  colorBelow: string
}
