import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { mergeMap, take } from 'rxjs/operators';
import { PaginatedResult } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserToken } from 'src/app/_models/user-token';
import { UserParams } from 'src/app/_models/user-params';
import { AccountService } from 'src/app/_services/account.service';
import { UsersService } from 'src/app/_services/users.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent implements OnInit {
  paginatedResult$: Observable<PaginatedResult<User>>;
  userParams: UserParams;
  userToken: UserToken;

  constructor(
    private usersService: UsersService,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.userParams = this.usersService.loadUserParams();
    this.loadMembers();
  }

  pageChanged(event) {
    if (this.userParams && this.userParams.currentPage !== event.page) {
      this.userParams.currentPage = event.page;
      this.loadMembers();
    }
  }

  resetFilters() {
    this.userParams = new UserParams(this.userToken);
    this.loadMembers();
  }

  loadMembers() {
    if (this.userParams) {
      if (!this.userToken) {
        this.accountService.currentUser$
          .pipe(take(1))
          .subscribe((userToken) => {
            this.userToken = userToken;
          });
      }
      this.usersService.saveUserParams(this.userParams);
      this.paginatedResult$ = this.usersService.getUsers(this.userParams);
    } else {
      this.paginatedResult$ = this.accountService.currentUser$.pipe(
        take(1),
        mergeMap((userToken) => {
          this.userToken = userToken;
          this.userParams = new UserParams(this.userToken);
          this.usersService.saveUserParams(this.userParams);
          return this.usersService.getUsers(this.userParams);
        })
      );
    }
  }
}
