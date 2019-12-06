import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Channelboard, ErrorCodes } from '../models/channelboard.models';
import { SettingsApi, ChannelboardService } from '../core';

@Component({
  selector: 'app-channelboard',
  templateUrl: './channelboard.component.html',
  styleUrls: ['./channelboard.component.sass']
})
export class ChannelboardComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private settings: SettingsApi,
    private channelboardService: ChannelboardService) { }

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

    this.channelboardService.getChannelboard(this.token)
      .subscribe(result => this.channelboard = result, (error: HttpErrorResponse) => {
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
