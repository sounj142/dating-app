import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';
import { MessageParams } from '../_models/user-params';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent implements OnInit {
  paginatedResult$: Observable<PaginatedResult<Message>>;
  params = new MessageParams();

  constructor(
    private messageService: MessageService,
    private confirmService: ConfirmService
  ) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.paginatedResult$ = this.messageService.getMessages(this.params);
  }

  pageChanged(event) {
    if (this.params && this.params.currentPage !== event.page) {
      this.params.currentPage = event.page;
      this.loadMessages();
    }
  }

  containerChanged() {
    this.params.currentPage = 1; //reset to page 1
    this.loadMessages();
  }

  getNavigateLink(message: Message) {
    return this.params.container === 'Outbox'
      ? `/members/${message.recipientUserName}`
      : `/members/${message.senderUserName}`;
  }

  getPhotoUrl(message: Message) {
    const photoUrl =
      this.params.container === 'Outbox'
        ? message.recipientPhotoUrl
        : message.senderPhotoUrl;
    return photoUrl || environment.defaultUserPhoto;
  }

  getDisplayName(message: Message) {
    return this.params.container === 'Outbox'
      ? message.recipientUserName
      : message.senderUserName;
  }

  deleteMessage(event, message: Message) {
    event.stopPropagation();
    this.confirmService
      .confirm('Are you sure you want to delete message?')
      .pipe(take(1))
      .subscribe((ok) => {
        if (ok) {
          this.messageService.deleteMessage(message.id).subscribe(() => {
            this.loadMessages();
          });
        }
      });
  }
}
