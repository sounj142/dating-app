import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { UserToken } from '../_models/user-token';
import { Observable, ReplaySubject } from 'rxjs';
import { LoginModel } from '../_models/login-model';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root',
})
export class AccountService extends BaseService {
  private currentUserSource = new ReplaySubject<UserToken>(1);

  currentUser$ = this.currentUserSource.asObservable();

  constructor(http: HttpClient) {
    super(http);
  }

  login(model: LoginModel) {
    return this.chainPipeToLoginOrRegisterRequest(
      this.http.post(`${this.baseUrl}account/login`, model)
    );
  }

  register(model) {
    return this.chainPipeToLoginOrRegisterRequest(
      this.http.post(`${this.baseUrl}account/register`, model)
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  getCurrentUserFromStorage() {
    const user: UserToken = JSON.parse(localStorage.getItem('user'));
    this.currentUserSource.next(user);
  }

  saveUserTokenToLocalStorage(user: UserToken) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  private chainPipeToLoginOrRegisterRequest(
    observable: Observable<Object>
  ): Observable<UserToken> {
    return observable.pipe(
      map((user: UserToken) => {
        if (user) {
          this.saveUserTokenToLocalStorage(user);
        }
        return user;
      })
    );
  }
}
