import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { User } from 'src/app/_models/user';
import { UsersService } from 'src/app/_services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit {
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  constructor(
    private usersService: UsersService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.usersService
      .getUser(this.route.snapshot.paramMap.get('userName'))
      .subscribe((user) => {
        this.user = user;
        this.galleryImages = this.getUsersImages();
      });
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];
  }

  getUserPhotoUrl() {
    return this.user?.photoUrl || environment.defaultUserPhoto;
  }

  private getUsersImages(): NgxGalleryImage[] {
    const imageUrls: NgxGalleryImage[] = [];
    if (this.user?.photos) {
      for (const photo of this.user.photos) {
        imageUrls.push({
          big: photo.url,
          small: photo.url,
          medium: photo.url,
        });
      }
    }
    return imageUrls;
  }
}
