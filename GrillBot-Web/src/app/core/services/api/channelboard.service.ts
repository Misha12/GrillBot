import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Channelboard } from 'src/app/models/channelboard.models';

@Injectable({ providedIn: 'root' })
export class ChannelboardService {
  constructor(
    @Inject('BaseUrl') private baseUrl: string,
    private httpClient: HttpClient
  ) { }

  getChannelboard(token: string): Observable<Channelboard> {
    return new Observable((observer) => {
      this.httpClient.get<any>(this.baseUrl + 'api/Channelboard/GetChannelboardData?token=' + token).subscribe(result => {
        observer.next(Channelboard.fromAny(result));
      });
    });
  }
}
