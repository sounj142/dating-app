import { Component, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { PhotoForAdminFeatureDto } from 'src/app/_models/user-and-role-info';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css'],
})
export class PhotoManagementComponent implements OnInit {
  private photosSource = new BehaviorSubject<PhotoForAdminFeatureDto[]>([]);
  photos$ = this.photosSource.asObservable();

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadPhotos();
  }

  loadPhotos() {
    this.adminService.getPhotosToModerates().subscribe((photos) => {
      this.photosSource.next(photos);
    });
  }

  approvePhoto(photo: PhotoForAdminFeatureDto) {
    this.adminService.approvePhoto(photo.id).subscribe(() => {
      this.removePhotoFromPipe(photo);
    });
  }

  rejectPhoto(photo: PhotoForAdminFeatureDto) {
    this.adminService.rejectPhoto(photo.id).subscribe(() => {
      this.removePhotoFromPipe(photo);
    });
  }

  private removePhotoFromPipe(photo: PhotoForAdminFeatureDto) {
    this.photos$.pipe(take(1)).subscribe((photos) => {
      this.photosSource.next(photos.filter((p) => p.id !== photo.id));
    });
  }
}
