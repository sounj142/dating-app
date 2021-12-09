import {
  AfterViewChecked,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { from, Subscription } from 'rxjs';
import { delay } from 'rxjs/operators';
import { DateReadDto, Message } from 'src/app/_models/message';
import { PresenceTrackerData, User } from 'src/app/_models/user';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { environment } from 'src/environments/environment';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent
  implements OnInit, OnDestroy, AfterViewChecked
{
  private trackerSubscription: Subscription;
  private messageSubscription: Subscription;
  private readMessageSubscription: Subscription;
  @ViewChild('memberTabs') memberTabs: TabsetComponent;
  @ViewChild('memberMessagesComponent')
  memberMessagesComponent: MemberMessagesComponent;
  activeTab: TabDirective;
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = [];
  initializedTab: boolean = false;
  haventGetMessages: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService,
    private presenceService: PresenceService,
    router: Router
  ) {
    router.routeReuseStrategy.shouldReuseRoute = () => false;
  }

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

    this.trackerSubscription = this.presenceService.presenceTracker$.subscribe(
      (trackerData: PresenceTrackerData) => {
        if (this.user.userName === trackerData.userName) {
          this.user.isOnline = trackerData.isOnline;
        }
      }
    );

    this.messageSubscription =
      this.messageService.messagesObservable$.subscribe((message: Message) => {
        if (
          message.senderId === this.user.id ||
          message.recipientId === this.user.id
        ) {
          this.messages.push(message);

          if (message.recipientId === this.user.id) {
            this.memberMessagesComponent.clearChatBox();
          } else {
            if (!message.dateRead && this.activeTabIsMessagesTab()) {
              this.messageService.markMessageAsRead([message.id]);
            }
          }
        }
      });

    this.readMessageSubscription =
      this.messageService.readMessageObservable$.subscribe(
        (dto: DateReadDto[]) => {
          for (const item of dto) {
            const msg = this.messages.find((m) => m.id === item.messageId);
            if (msg) msg.dateRead = item.dateRead;
          }
        }
      );
  }

  ngOnDestroy(): void {
    this.trackerSubscription.unsubscribe();
    this.messageSubscription.unsubscribe();
    this.readMessageSubscription.unsubscribe();
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
    if (this.activeTabIsMessagesTab()) {
      if (this.haventGetMessages) {
        this.haventGetMessages = false;
        this.loadMessages();
      } else {
        this.markUnreadMessagesAsRead();
      }
    }
  }

  activeTabIsMessagesTab() {
    return this.activeTab.heading === 'Messages';
  }

  loadMessages() {
    this.messageService
      .getMessagesThread(this.user.id)
      .subscribe((messages) => {
        this.messages = messages;

        this.markUnreadMessagesAsRead();
      });
  }

  markUnreadMessagesAsRead() {
    const unreadMessageIds = this.messages
      .filter((m) => !m.dateRead && m.senderId === this.user.id)
      .map((m) => m.id);

    if (unreadMessageIds.length) {
      this.messageService.markMessageAsRead(unreadMessageIds);
    }
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }
}
