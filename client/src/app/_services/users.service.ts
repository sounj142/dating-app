import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CacheItem } from '../_models/cache-item';
import { PaginatedResult } from '../_models/pagination';
import { PresenceTrackerData, User } from '../_models/user';
import { UserLiked } from '../_models/user-liked';
import { LikesParams, UserParams } from '../_models/user-params';
import { BaseService } from './base.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root',
})
export class UsersService extends BaseService {
  private memberCache = {};
  private cachedUserParams: UserParams = undefined;

  constructor(http: HttpClient, private presenceService: PresenceService) {
    super(http);
    this.trackUsersPresence();
  }

  private trackUsersPresence() {
    this.presenceService.presenceTracker$.subscribe(
      (trackerData: PresenceTrackerData) => {
        for (const key in this.memberCache) {
          const cacheItem: CacheItem<PaginatedResult<User>> =
            this.memberCache[key];

          if (this.isValidCachedItem(cacheItem)) {
            const user = cacheItem.data.data.find(
              (u) => u.userName === trackerData.userName
            );
            if (user) {
              user.isOnline = trackerData.isOnline;
            }
          }
        }
      }
    );
  }

  private getCacheKey(p: any) {
    return Object.values(p).join('-');
  }

  private isValidCachedItem<T>(item: CacheItem<T>): boolean {
    return item && item.timeExpired > new Date();
  }

  saveUserParams(userParams: UserParams) {
    this.cachedUserParams = this.cloneObject(userParams);
  }

  loadUserParams() {
    return this.cachedUserParams
      ? this.cloneObject(this.cachedUserParams)
      : undefined;
  }

  clearMemberCache() {
    this.memberCache = {};
  }

  getUsers(userParams: UserParams): Observable<PaginatedResult<User>> {
    const key = this.getCacheKey(userParams);
    const cacheItem: CacheItem<PaginatedResult<User>> = this.memberCache[key];

    if (this.isValidCachedItem(cacheItem)) {
      return of(this.cloneObject(cacheItem.data));
    }

    return this.getPaginationResult<User, UserParams>(
      `${this.baseUrl}users`,
      userParams
    ).pipe(
      map((result) => {
        const timeExpired = new Date();
        timeExpired.setMinutes(
          timeExpired.getMinutes() + environment.cacheTime
        );

        this.memberCache[key] = {
          data: this.cloneObject(result),
          timeExpired: timeExpired,
        };
        return result;
      })
    );
  }

  getUser(userName: string) {
    for (const key in this.memberCache) {
      const cacheItem: CacheItem<PaginatedResult<User>> = this.memberCache[key];

      if (this.isValidCachedItem(cacheItem)) {
        const user = cacheItem.data.data.find((u) => u.userName === userName);
        if (user) {
          return of(this.cloneObject(user));
        }
      }
    }

    return this.http.get<User>(`${this.baseUrl}users/${userName}`);
  }

  getUserForEdit() {
    return this.http.get<User>(`${this.baseUrl}users/for-edit`);
  }

  updateUser(user: User) {
    return this.http.put(`${this.baseUrl}users`, user);
  }

  setMainPhoto(photoId: number) {
    return this.http.put(
      `${this.baseUrl}users/set-main-photo/${photoId}`,
      undefined
    );
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`);
  }

  addLike(userName: string) {
    return this.http.post(`${this.baseUrl}likes/${userName}`, undefined);
  }

  getLikes(likesParams: LikesParams) {
    return this.getPaginationResult<UserLiked, LikesParams>(
      `${this.baseUrl}likes`,
      likesParams
    );
  }
}
