import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from 'src/app/_models/user';
import { UsersService } from 'src/app/_services/users.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users$: Observable<User[]>;

  constructor(private usersService: UsersService) { }

  ngOnInit(): void {
    this.users$ = this.usersService.getUsers();
  }
}
