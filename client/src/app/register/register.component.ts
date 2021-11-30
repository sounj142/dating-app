import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { RegisterModel } from '../_models/register-model';
import { UserToken } from '../_models/user-token';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  model: RegisterModel = {};

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  register() {
    this.accountService.register(this.model).subscribe(
      (user: UserToken) => {
        console.log(user);
        this.cancel();
      },
      (response) => {
        this.toastr.error(response.error);
        console.log(response);
      }
    );
  }

  cancel() {
    this.cancelRegister.emit();
  }
}
