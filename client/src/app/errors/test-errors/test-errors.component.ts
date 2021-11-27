import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css'],
})
export class TestErrorsComponent implements OnInit {
  private baseUrl = 'https://localhost:44312/api/buggy/';

  validationErrors: string[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {}

  get404Error() {
    this.http
      .get(`${this.baseUrl}not-found`)
      .subscribe(console.log, (error) => {
        console.log(error);
      });
  }

  get400Error() {
    this.http
      .get(`${this.baseUrl}bad-request`)
      .subscribe(console.log, (error) => {
        console.log(error);
      });
  }

  get500Error() {
    this.http
      .get(`${this.baseUrl}server-error`)
      .subscribe(console.log, (error) => {
        console.log(error);
      });
  }

  get401Error() {
    this.http.get(`${this.baseUrl}auth`).subscribe(console.log, (error) => {
      console.log(error);
    });
  }

  get400ValidationError() {
    this.http
      .post(`https://localhost:44312/api/account/register`, {})
      .subscribe(console.log, (error) => {
        this.validationErrors = error;
        console.log(error);
      });
  }
}
