import { Injectable } from '@angular/core';
import {
  CanDeactivate,
} from '@angular/router';
import { IPreventUnsavedChangesComponent } from '../_interfaces/i-prevent-unsaved-changes-component';

@Injectable({
  providedIn: 'root',
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  canDeactivate(component: IPreventUnsavedChangesComponent): boolean {
    if (component.hasUnsavedChanges()){
      return confirm('Are you sure you want to continue? Any unsaved changes will be lost!');
    }
    return true;
  }
}
