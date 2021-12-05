import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from '../_models/pagination';

export class BaseService {
  protected baseUrl = environment.apiUrl;

  constructor(protected http: HttpClient) {}

  protected getPaginationResult<T, TParams>(
    url,
    userParams: TParams
  ): Observable<PaginatedResult<T>> {
    let params = new HttpParams();
    if (userParams) {
      for (const key in userParams) {
        params = params.append(key, String(userParams[key]));
      }
    }
    return this.http
      .get<T[]>(url, {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          const paginatedResult = new PaginatedResult<T>();
          paginatedResult.data = response.body;
          if (response.headers.get('Pagination')) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get('Pagination')
            );
          }
          return paginatedResult;
        })
      );
  }

  protected cloneObject<T>(obj: T): T {
    return JSON.parse(JSON.stringify(obj));
  }

}
