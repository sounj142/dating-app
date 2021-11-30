import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { UserToken } from '../_models/user-token';
import { Observable, ReplaySubject } from 'rxjs';
import { LoginModel } from '../_models/login-model';
import { RegisterModel } from '../_models/register-model';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<UserToken>(1);

  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) {}

  login(model: LoginModel) {
    return this.chainPipeToLoginOrRegisterRequest(
      this.http.post(`${this.baseUrl}account/login`, model)
    );
  }

  register(model: RegisterModel) {
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

  private chainPipeToLoginOrRegisterRequest(
    observable: Observable<Object>
  ): Observable<UserToken> {
    return observable.pipe(
      map((user: UserToken) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        return user;
      })
    );
  }
}
