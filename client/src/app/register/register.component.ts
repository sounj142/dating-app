import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { UserToken } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  model: any = {};

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {}

  register() {
    this.accountService.register(this.model).subscribe((user: UserToken) => {
      console.log(user);
      this.cancel();
    }, console.log);
  }

  cancel() {
    this.cancelRegister.emit();
  }
}
