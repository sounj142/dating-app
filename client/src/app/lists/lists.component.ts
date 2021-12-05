import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { UserLiked } from '../_models/user-liked';
import { LikesParams } from '../_models/user-params';
import { UsersService } from '../_services/users.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css'],
})
export class ListsComponent implements OnInit {
  paginatedResult$: Observable<PaginatedResult<UserLiked>>;
  likesParams = new LikesParams();

  constructor(private usersService: UsersService) {}

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.paginatedResult$ = this.usersService.getLikes(this.likesParams);
  }

  changePredicate() {
    this.likesParams.currentPage = 1; //reset to page 1
    this.loadMembers();
  }

  pageChanged(event) {
    if (this.likesParams && this.likesParams.currentPage !== event.page) {
      this.likesParams.currentPage = event.page;
      this.loadMembers();
    }
  }
}
