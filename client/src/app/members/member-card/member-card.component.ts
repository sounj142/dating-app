import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { User } from 'src/app/_models/user';
import { UsersService } from 'src/app/_services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent implements OnInit {
  @Input() user: User;

  constructor(
    private usersService: UsersService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  getUserPhotoUrl() {
    return this.user?.photoUrl || environment.defaultUserPhoto;
  }

  addLike() {
    this.usersService.addLike(this.user.userName).subscribe(() => {
      this.toastr.success(`You have liked ${this.user.knownAs}`);
    });
  }
}
