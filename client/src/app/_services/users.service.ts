import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { UserUpdate } from '../_models/user-update';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private baseUrl = environment.apiUrl;
  private users: User[];

  constructor(private http: HttpClient) {}

  getUsers() {
    if (this.users) {
      return of(this.users);
    }
    return this.http.get<User[]>(`${this.baseUrl}users`).pipe(
      map((users) => {
        if (users) {
          this.users = users;
        }
        return users;
      })
    );
  }

  getUser(userName: string) {
    if (this.users) {
      const userNameLowerCase = userName?.toLowerCase();
      const user = this.users.find(
        (u) => u.userName.toLowerCase() === userNameLowerCase
      );
      if (user) {
        return of(user);
      }
    }
    return this.http.get<User>(`${this.baseUrl}users/${userName}`);
  }

  updateUser(user: User) {
    return this.http.put(`${this.baseUrl}users`, user).pipe(
      map((value) => {
        if (this.users) {
          const index = this.users.findIndex(
            (u) => u.userName === user.userName
          );
          if (index >= 0) {
            this.users[index] = user;
          }
        }
        return value;
      })
    );
  }
}
