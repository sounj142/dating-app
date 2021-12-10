import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { IPreventUnsavedChangesComponent } from 'src/app/_interfaces/i-prevent-unsaved-changes-component';
import { User } from 'src/app/_models/user';
import { UsersService } from 'src/app/_services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css'],
})
export class MemberEditComponent implements OnInit, IPreventUnsavedChangesComponent {
  @ViewChild('editForm') editForm: NgForm;
  user: User;

  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if(this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private usersService: UsersService,
    private toastr: ToastrService
  ) {}

  hasUnsavedChanges(): boolean {
    return this.editForm && this.editForm.dirty;
  }

  ngOnInit(): void {
    this.usersService.getUserForEdit().subscribe((user) => {
      this.user = user;
    });
  }

  getUserPhotoUrl() {
    return this.user?.photoUrl || environment.defaultUserPhoto;
  }

  updateUser() {
    this.usersService.updateUser(this.user).subscribe(() => {
      this.toastr.success('Profile updated successfully');
      this.editForm.reset(this.user);
    });
  }
}
