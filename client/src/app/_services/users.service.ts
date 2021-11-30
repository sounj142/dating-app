import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<User[]>(`${this.baseUrl}users`);
  }

  getUser(userName: string) {
    return this.http.get<User>(`${this.baseUrl}users/${userName}`);
  }
}
