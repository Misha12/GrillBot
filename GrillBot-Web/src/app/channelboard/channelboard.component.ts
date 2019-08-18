import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Channelboard, ErrorCodes } from '../models/channelboard.models';
import { GetCommandPrefix, SettingsApi } from '../core';

@Component({
  selector: 'app-channelboard',
  templateUrl: './channelboard.component.html',
  styleUrls: ['./channelboard.component.sass']
})
export class ChannelboardComponent implements OnInit {
  constructor(private httpClient: HttpClient,
              private route: ActivatedRoute,
              @Inject('BaseUrl') private baseUrl: string,
              private settings: SettingsApi) { }

  private unspecifiedError = 'Došlo k nespecifikované chybě';
  private missingToken = 'Nebyl zadán uživatelský token.';
  private invalidToken = 'Byl zadán neplatný token.';
  private botRequestMessage = ' Požádej bota o nový token příkazem {command}.';

  public token: string;
  public channelboard: Channelboard;

  public errorMessage: string;

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token');

    this.settings.getCommandPrefix().subscribe(data => {
      this.botRequestMessage = this.botRequestMessage.replace('{command}', data.commandPrefix + 'channelboardweb');

      this.getChannelBoardData();
    });
  }

  private getChannelBoardData() {
    if (this.token == null) {
      this.errorMessage = this.missingToken + this.botRequestMessage;
      return;
    }

    this.httpClient.get<any>(this.baseUrl + 'api/Channelboard/GetChannelboardData?token=' + this.token).subscribe(result => {
      this.channelboard = Channelboard.fromAny(result);
    }, (error: HttpErrorResponse) => {
      if (!error.error) {
        this.errorMessage = this.unspecifiedError;
      } else {
        switch (error.error.code) {
          case ErrorCodes.InvalidToken: this.errorMessage = this.invalidToken + this.botRequestMessage; break;
          case ErrorCodes.MissingToken: this.errorMessage = this.missingToken + this.botRequestMessage; break;
          default: this.errorMessage = this.unspecifiedError; break;
        }
      }
    });
  }
}
