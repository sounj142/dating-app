import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';
import { IPreventUnsavedChangesComponent } from '../_interfaces/i-prevent-unsaved-changes-component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root',
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  constructor(private confirmService: ConfirmService) {}

  canDeactivate(
    component: IPreventUnsavedChangesComponent
  ): Observable<boolean> | boolean {
    if (component.hasUnsavedChanges()) {
      return this.confirmService.confirm(
        'Are you sure you want to continue? Any unsaved changes will be lost!'
      );
    }
    return true;
  }
}
