import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { MessageService } from './_services/message.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'My Dating App';

  constructor(
    private accountService: AccountService,
    _p: PresenceService,
    _m: MessageService
  ) {}

  ngOnInit(): void {
    this.accountService.getCurrentUserFromStorage();
  }
}
