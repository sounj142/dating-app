import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error) {
          switch (error.status) {
            case 400:
              this.process400Error(error);
              break;
            case 401:
              this.process401Error(error);
              break;
            case 404:
              this.process404Error();
              break;
            case 500:
              this.process500Error(error);
              break;
            default:
              this.toastr.error('Something unexpected went wrong', String(error.status));
              console.log(error);
              break;
          }
        }
        return throwError(error);
      })
    );
  }

  private process400Error(error: HttpErrorResponse) {
    if (error.error) {
      if (typeof error.error === 'string') {
        this.toastr.error(error.error, String(error.status));
        return;
      } else if (error.error.errors) {
        const errors = error.error.errors;
        const modelStateErrors = [];
        for (let key in errors) {
          const propertyError = errors[key];
          if (propertyError && propertyError.length) {
            modelStateErrors.push(...propertyError);
          }
        }
        throw modelStateErrors;
      } else if (error.error.title) {
        this.toastr.error(error.error.title, String(error.status));
        return;
      }
    }

    this.toastr.error(error.statusText, String(error.status));
  }

  private process401Error(error: HttpErrorResponse) {
    this.toastr.error(error.error || 'Unauthorized access!', String(error.status));
  }

  private process404Error() {
    this.router.navigateByUrl('/not-found');
  }

  private process500Error(error: HttpErrorResponse) {
    const navigationExtras: NavigationExtras = {
      state: { error: error.error },
    };
    this.router.navigateByUrl('/server-error', navigationExtras);
  }
}
