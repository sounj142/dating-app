import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { getClientTimezoneOffset } from '../_fn/date-function';

@Injectable()
export class AddClientTimezoneInterceptor implements HttpInterceptor {
  constructor() {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    request = request.clone({
      setHeaders: {
        ClientTimeZoneOffset: getClientTimezoneOffset().toString(),
      },
    });
    return next.handle(request);
  }
}
