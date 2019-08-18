import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GetCommandPrefix } from '../../models';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class SettingsApi {
    constructor(@Inject('BaseUrl') private baseUrl: string,
                private httpClient: HttpClient) { }

    getCommandPrefix(): Observable<GetCommandPrefix> {
        return this.httpClient.get<GetCommandPrefix>(this.baseUrl + 'api/Settings/GetCommandPrefix');
    }
}
