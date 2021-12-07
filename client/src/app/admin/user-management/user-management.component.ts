import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import {
  RoleCheckboxData,
  UserAndRolesInfo,
} from 'src/app/_models/user-and-role-info';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css'],
})
export class UserManagementComponent implements OnInit {
  userAndRoles$: Observable<UserAndRolesInfo[]>;
  modalRef: BsModalRef;

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.userAndRoles$ = this.adminService.getUsersWithRoles();
  }

  editRoles(user: UserAndRolesInfo, roles: RoleCheckboxData[]) {
    const newRoles = roles.filter((r) => r.checked).map((x) => x.name);
    this.adminService.editRoles(user.userName, newRoles).subscribe(() => {
      this.toastr.success(`Update roles succeed for user ${user.userName}`);
      user.roles = newRoles;
    });
  }

  openRolesModal(user: UserAndRolesInfo) {
    const config: ModalOptions = {
      class: 'modal-dialog-centered',
      initialState: {
        user: user,
        roles: [
          {
            name: 'Admin',
            checked: user.roles.some((s) => s === 'Admin'),
          },
          {
            name: 'Moderator',
            checked: user.roles.some((s) => s === 'Moderator'),
          },
          {
            name: 'Member',
            checked: user.roles.some((s) => s === 'Member'),
          },
        ],
      },
    };

    this.modalRef = this.modalService.show(RolesModalComponent, config);
    this.modalRef.content.updateSelectedRoles.subscribe((roles) => {
      this.editRoles(user, roles);
    });
  }
}
