import { Component, Input, OnInit } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { MessageService } from 'src/app/_services/message.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @Input() user: User;
  @Input() messages: Message[];
  messageContent: string;

  constructor(private messageService: MessageService) {}

  ngOnInit(): void {
    console.log('MemberMessagesComponent init');
  }

  getPhotoUrl(message: Message) {
    return message.senderPhotoUrl || environment.defaultUserPhoto;
  }

  sendMessage() {
    this.messageService.sendMessage(this.user.userName, this.messageContent);
  }

  clearChatBox() {
    this.messageContent = '';
  }
}
