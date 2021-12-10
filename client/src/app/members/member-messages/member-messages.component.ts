import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { MessageService } from 'src/app/_services/message.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberMessagesComponent implements OnInit {
  @Input() user: User;
  @ViewChild('messageForm') messageForm: NgForm;
  messageContent: string;

  constructor(public messageService: MessageService) {}

  ngOnInit(): void {}

  getPhotoUrl(message: Message) {
    return message.senderPhotoUrl || environment.defaultUserPhoto;
  }

  sendMessage() {
    this.messageService.sendMessage(this.user.userName, this.messageContent).then(() => {
      this.messageForm.reset();
    });
  }
}
