import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { getClientTimezoneOffset } from '../_fn/date-function';
import { CreateMessageDto, DateReadDto, Message } from '../_models/message';
import { MessageParams } from '../_models/user-params';
import { UserToken } from '../_models/user-token';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root',
})
export class MessageService extends BaseService {
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThreadObservable$ = this.messageThreadSource.asObservable();

  constructor(http: HttpClient, private toastr: ToastrService) {
    super(http);
  }

  getMessages(messageParams: MessageParams) {
    return this.getPaginationResult<Message, MessageParams>(
      `${this.baseUrl}messages`,
      messageParams
    );
  }

  sendMessage(userName: string, content: string) {
    const message: CreateMessageDto = {
      recipientUserName: userName,
      content: content,
    };

    return this.hubConnection.invoke('SendMessage', message).catch((error) => {
      console.log(error);
      this.toastr.error(
        'Error occurred when sending message, please try again later.'
      );
      return error;
    });
  }

  createHubConnection(user: UserToken, recipientUserName: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(
        `${
          this.hubUrl
        }message?Recipient=${recipientUserName}&ClientTimezoneOffset=${getClientTimezoneOffset()}`,
        {
          accessTokenFactory: () => user.token,
        }
      )
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('SendServerErrorMessage', (msg: string) => {
      this.toastr.error(msg);
    });

    this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('ReceivedMessage', (message: Message) => {
      this.messageThreadObservable$.pipe(take(1)).subscribe((messages) => {
        this.messageThreadSource.next([...messages, message]);
      });
    });

    this.hubConnection.on('MessagesIsRead', (readMsgs: DateReadDto[]) => {
      this.messageThreadObservable$.pipe(take(1)).subscribe((messages) => {
        for (const item of readMsgs) {
          const msg = messages.find((m) => m.id === item.messageId);
          if (msg) msg.dateRead = item.dateRead;
        }
        this.messageThreadSource.next([...messages]);
      });
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
