import { Component, Input, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { take } from 'rxjs/operators';
import { AccountService } from 'src/app/_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { UsersService } from 'src/app/_services/users.service';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css'],
})
export class PhotoEditorComponent implements OnInit {
  @Input() user: User;
  uploader: FileUploader;
  hasBaseDropzoneOver: boolean = false;
  baseUrl = environment.apiUrl;

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private usersService: UsersService
  ) {}

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: boolean) {
    this.hasBaseDropzoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}users/add-photo`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (_item, response, _status, _headers) => {
      if (!response) {
        this.toastr.error('Error occured when uploading photo');
        return;
      }

      const photo: Photo = JSON.parse(response);
      this.user.photos.push(photo);

      if (photo.isMain) this.changeMainPhotoUrl(photo);
    };

    this.accountService.currentUser$.pipe(take(1)).subscribe((userToken) => {
      if (!userToken?.token) return;
      this.uploader.authToken = `Bearer ${userToken.token}`;
    });
  }

  setMainPhoto(photo: Photo) {
    this.usersService.setMainPhoto(photo.id).subscribe(() => {
      this.user.photos.forEach((p) => (p.isMain = false));
      photo.isMain = true;
      this.changeMainPhotoUrl(photo);
    });
  }

  deletePhoto(photo: Photo) {
    this.usersService.deletePhoto(photo.id).subscribe(() => {
      this.user.photos = this.user.photos.filter((p) => p.id !== photo.id);
    });
  }

  private changeMainPhotoUrl(photo: Photo) {
    this.user.photoUrl = photo.url;

    // set photo in User token
    this.accountService.currentUser$.pipe(take(1)).subscribe((userToken) => {
      userToken.photoUrl = photo.url;
      this.accountService.saveUserTokenToLocalStorage(userToken);
    });
  }
}
