import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { getClientTimezoneOffset } from '../_fn/date-function';
import {
  CreateMessageDto,
  DateReadDto,
  MarkMessageAsReadDto,
  Message,
  SendMessageResult,
} from '../_models/message';
import { MessageParams } from '../_models/user-params';
import { UserToken } from '../_models/user-token';
import { AccountService } from './account.service';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root',
})
export class MessageService extends BaseService {
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messagesSource = new Subject<Message>();
  messagesObservable$ = this.messagesSource.asObservable();
  private readMessageSource = new Subject<DateReadDto[]>();
  readMessageObservable$ = this.readMessageSource.asObservable();

  constructor(
    http: HttpClient,
    accountService: AccountService,
    private toastr: ToastrService,
    private router: Router
  ) {
    super(http);

    accountService.currentUser$.subscribe((user) => {
      if (user?.token) {
        this.createHubConnection(user);
      } else {
        this.stopHubConnection();
      }
    });
  }

  getMessages(messageParams: MessageParams) {
    return this.getPaginationResult<Message, MessageParams>(
      `${this.baseUrl}messages`,
      messageParams
    );
  }

  getMessagesThread(userId: number) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${userId}`);
  }

  sendMessage(userName: string, content: string): void {
    const message: CreateMessageDto = {
      recipientUserName: userName,
      content: content,
      clientTimezoneOffset: getClientTimezoneOffset(),
    };

    this.hubConnection.send('SendMessage', message).catch((error) => {
      console.log(error);
      this.toastr.error(
        'Error occurred when sending message, please try again later.'
      );
      return error;
    });
  }

  markMessageAsRead(messageIds: number[]): void {
    const data: MarkMessageAsReadDto = {
      messageIds: messageIds,
      clientTimezoneOffset: getClientTimezoneOffset(),
    };

    this.hubConnection.send('MarkMessageAsRead', data).catch((error) => {
      console.error(error);
    });
  }

  createHubConnection(user: UserToken) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}message`, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('SendMessageResult', (result: SendMessageResult) => {
      if (result.succeeded) {
        this.messagesSource.next(result.message);
      } else {
        this.toastr.error(result.error);
      }
    });

    this.hubConnection.on('ReceivedMessage', (message: Message) => {
      this.messagesSource.next(message);
    });

    this.hubConnection.on('MessageIsRead', (message: DateReadDto[]) => {
      this.readMessageSource.next(message);
    });

    this.hubConnection.on('MessageNotification', (data: {userName: string, knownAs: string}) => {
      this.toastr.info(`${data.knownAs} has sent you a new message`)
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl(`/members/${data.userName}?tab=3`));
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

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`);
  }
}
