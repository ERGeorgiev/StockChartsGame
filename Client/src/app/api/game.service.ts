import { HttpClient, HttpContext, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Guid } from 'guid-typescript';
import { env } from '../../environments/environment';
import { Quote } from '../chart/chart.models';

@Injectable()
export class GameService {
  sessionId: string;

  constructor(private readonly http: HttpClient) {
    this.sessionId = this.generateSessionId();
    console.log(this.sessionId);
  }

  refreshQuotes() {
    return this.http.post(`${env.api}/refreshQuotes`, this.requestHeader());
  }

  getSymbol() {
    return this.http.get(`${env.api}/symbol`, this.requestHeader());
  }

  reveal() {
    return this.http.post(`${env.api}/reveal`, this.requestHeader());
  }

  getQuotes() {
    return this.http.get<Quote[]>(`${env.api}/quotes`, this.requestHeader());
  }

  requestHeader(): { headers?: HttpHeaders } {
    let headers = new HttpHeaders();
    headers.append('Content-Type', 'application/json');

    return { headers: headers };
  }

  generateSessionId(): string {
    return `sessionId-${Guid.create().toString()}`;
  }
}
