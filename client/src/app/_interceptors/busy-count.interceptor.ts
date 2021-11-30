import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { delay, finalize } from 'rxjs/operators';
import { BusyService } from '../_services/busy.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class BusyCountInterceptor implements HttpInterceptor {
  constructor(private busyService: BusyService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    this.busyService.busy();

    if (environment.fakeDelayTime) {
      return next.handle(request).pipe(
        delay(environment.fakeDelayTime),
        finalize(() => {
          this.busyService.idle();
        })
      );
    } else {
      return next.handle(request).pipe(
        finalize(() => {
          this.busyService.idle();
        })
      );
    }
  }
}
