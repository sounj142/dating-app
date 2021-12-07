import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {
  RoleCheckboxData,
  UserAndRolesInfo,
} from 'src/app/_models/user-and-role-info';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css'],
})
export class RolesModalComponent implements OnInit {
  @Input() updateSelectedRoles = new EventEmitter<RoleCheckboxData[]>();
  user: UserAndRolesInfo;
  roles: RoleCheckboxData[];

  constructor(public modalRef: BsModalRef) {}

  ngOnInit(): void {}

  updateRoles() {
    this.updateSelectedRoles.emit(this.roles);
    this.modalRef.hide();
  }
}
