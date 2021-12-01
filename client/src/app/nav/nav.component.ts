import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { LoginModel } from '../_models/login-model';
import { UserToken } from '../_models/user-token';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent {
  model: LoginModel = {};

  constructor(public accountService: AccountService, private router: Router) {}

  login() {
    this.accountService.login(this.model).subscribe((_) => {
      this.router.navigateByUrl('/members');
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  getUserPhotoUrl(user: UserToken) {
    return user?.photoUrl || environment.defaultUserPhoto
  }
}
