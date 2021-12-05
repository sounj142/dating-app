import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Message } from '../_models/message';
import { MessageParams } from '../_models/user-params';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root',
})
export class MessageService extends BaseService {
  constructor(http: HttpClient) {
    super(http);
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

  sendMessage(recipientUserName: string, content: string) {
    return this.http.post<Message>(`${this.baseUrl}messages`, {
      recipientUserName,
      content,
    });
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`);
  }
}
