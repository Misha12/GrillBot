import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Channelboard } from '../models/channelboard.models';

@Component({
  selector: 'app-channelboard',
  templateUrl: './channelboard.component.html',
  styleUrls: ['./channelboard.component.sass']
})
export class ChannelboardComponent implements OnInit {
  constructor(private httpClient: HttpClient, private route: ActivatedRoute, @Inject('BaseUrl') private baseUrl: string) { }

  public token: string;
  public channelboard: Channelboard;

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token');
    this.getChannelBoardData();
  }

  private getChannelBoardData() {
    if (this.token == null) {
      console.error('Missing token');
      return;
    }

    this.httpClient.get<any>(this.baseUrl + 'api/Channelboard/GetChannelboardData?token=' + this.token).subscribe(result => {
      this.channelboard = Channelboard.fromAny(result);
      console.log(this.channelboard);
    }, error => {
      console.error(error);
      alert(JSON.stringify(error));
    });
  }
}
