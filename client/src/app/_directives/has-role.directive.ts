import {
  Directive,
  Input,
  OnInit,
  TemplateRef,
  ViewContainerRef,
} from '@angular/core';
import { take } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]',
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user) => {
      if (!user?.roles) {
        this.viewContainerRef.clear();
        return;
      }
      
      if (!user.roles.some((x) => this.appHasRole.includes(x))) {
        this.viewContainerRef.clear();
        return;
      }

      this.viewContainerRef.createEmbeddedView(this.templateRef);
    });
  }
}
