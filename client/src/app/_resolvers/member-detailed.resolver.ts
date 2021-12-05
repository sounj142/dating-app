import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { UsersService } from '../_services/users.service';

@Injectable({
  providedIn: 'root',
})
export class MemberDetailedResolver implements Resolve<User> {
  constructor(private usersService: UsersService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<User> {
    return this.usersService.getUser(route.paramMap.get('userName'));
  }
}
