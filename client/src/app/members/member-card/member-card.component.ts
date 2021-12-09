import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { PresenceTrackerData, User } from 'src/app/_models/user';
import { PresenceService } from 'src/app/_services/presence.service';
import { UsersService } from 'src/app/_services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent implements OnInit, OnDestroy {
  private trackerSubscription: Subscription;
  @Input() user: User;

  constructor(
    private usersService: UsersService,
    private toastr: ToastrService,
    private presenceService: PresenceService
  ) {}

  ngOnInit(): void {
    this.trackerSubscription = this.presenceService.presenceTracker$.subscribe(
      (trackerData: PresenceTrackerData) => {
        if (this.user.userName === trackerData.userName) {
          this.user.isOnline = trackerData.isOnline;
        }
      }
    );
  }

  ngOnDestroy(): void {
    this.trackerSubscription.unsubscribe();

  }

  getUserPhotoUrl() {
    return this.user?.photoUrl || environment.defaultUserPhoto;
  }

  addLike() {
    this.usersService.addLike(this.user.userName).subscribe(() => {
      this.toastr.success(`You have liked ${this.user.knownAs}`);
    });
  }
}
