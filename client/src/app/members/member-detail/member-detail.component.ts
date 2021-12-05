import { AfterViewChecked, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { from, Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { MessageService } from 'src/app/_services/message.service';
import { UsersService } from 'src/app/_services/users.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit, AfterViewChecked {
  @ViewChild('memberTabs') memberTabs: TabsetComponent;
  activeTab: TabDirective;
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = [];
  initializedTab: boolean = false;
  haventGetMessages: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService
  ) {}

  ngAfterViewChecked(): void {
    if (this.memberTabs?.tabs?.length && !this.initializedTab) {
      this.initializedTab = true;

      from([0])
        .pipe(delay(0))
        .subscribe(() => {
          this.selectInitialTabUsingQueryParams();
        });
    }
  }

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.user = data.user;
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

  selectInitialTabUsingQueryParams() {
    this.route.queryParams.subscribe((params) => {
      if (params.tab) {
        this.selectTab(params.tab);
      }
    });
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

  onTabActivated(tab: TabDirective) {
    this.activeTab = tab;
    if (this.activeTab.heading === 'Messages' && this.haventGetMessages) {
      this.haventGetMessages = false;
      this.loadMessages();
    }
  }

  loadMessages() {
    this.messageService
      .getMessagesThread(this.user.id)
      .subscribe((messages) => {
        this.messages = messages;
      });
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }
}
