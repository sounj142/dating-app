import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserAndRolesInfo } from '../_models/user-and-role-info';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root',
})
export class AdminService extends BaseService {
  constructor(http: HttpClient) {
    super(http);
  }

  getUsersWithRoles() {
    return this.http.get<UserAndRolesInfo[]>(
      `${this.baseUrl}admin/users-with-roles`
    );
  }

  editRoles(userName: string, roles: string[]) {
    return this.http.put(`${this.baseUrl}admin/edit-roles`, {
      userName,
      roles,
    });
  }
}
