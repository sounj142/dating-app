import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { PresenceTrackerData } from '../_models/user';
import { UserToken } from '../_models/user-token';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private presenceTrackerSource = new Subject<PresenceTrackerData>();
  presenceTracker$ = this.presenceTrackerSource.asObservable();

  constructor(private toastr: ToastrService, accountService: AccountService) {
    accountService.currentUser$.subscribe((user) => {
      if (user?.token) {
        this.createHubConnection(user);
      } else {
        this.stopHubConnection();
      }
    });
  }

  createHubConnection(user: UserToken) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('UserIsOnline', (userName) => {
      this.presenceTrackerSource.next({ userName, isOnline: true });
    });

    this.hubConnection.on('UserIsOffline', (userName) => {
      this.presenceTrackerSource.next({ userName, isOnline: false });
    });

    this.hubConnection.start().catch((error) => {
      console.log(error);
      return error;
    });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop().catch((error) => {
        console.log(error);
        return error;
      });
      this.hubConnection = null;
    }
  }
}
