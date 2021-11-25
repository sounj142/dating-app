import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'My Dating App';
  users: any;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getUsers();
  }

  private getUsers() {
    this.http.get('https://localhost:44312/api/Users')
      .subscribe(response => {
        this.users = response;
      }, error => {
        console.log(error);
      });
  }
}
