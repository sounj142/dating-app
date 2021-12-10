import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { PresenceTrackerData } from '../_models/user';
import { UserToken, UserUpdateNoticationData } from '../_models/user-token';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private presenceTrackerSource = new Subject<PresenceTrackerData>();
  presenceTracker$ = this.presenceTrackerSource.asObservable();

  constructor(
    private accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {
    accountService.currentUser$.pipe(take(1)).subscribe((user) => {
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

    this.hubConnection.on(
      'NewMessageNotification',
      (data: { userName: string; knownAs: string }) => {
        this.toastr
          .info(`${data.knownAs} has sent you a new message`)
          .onTap.pipe(take(1))
          .subscribe(() =>
            this.router.navigateByUrl(`/members/${data.userName}?tab=3`)
          );
      }
    );

    this.hubConnection.on(
      'UserInfoChanged',
      (data: UserUpdateNoticationData) => {
        user.photoUrl = data.photoUrl;
        user.knownAs = data.knownAs;
        user.gender = data.gender;

        this.accountService.saveUserTokenToLocalStorage(user);
      }
    );

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
