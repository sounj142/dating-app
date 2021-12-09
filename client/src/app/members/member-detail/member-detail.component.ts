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
import { PresenceTrackerData, User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent
  implements OnInit, OnDestroy, AfterViewChecked
{
  private trackerSubscription: Subscription;
  @ViewChild('memberTabs') memberTabs: TabsetComponent;
  activeTab: TabDirective;
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  initializedTab: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService,
    private presenceService: PresenceService,
    private accountService: AccountService,
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
  }

  ngOnDestroy(): void {
    this.trackerSubscription.unsubscribe();
    this.messageService.stopHubConnection();
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
      this.accountService.currentUser$.subscribe((userToken) => {
        if (userToken?.token) {
          this.messageService.createHubConnection(userToken, this.user.userName);
        }
      });
    } else {
      this.messageService.stopHubConnection();
    }
  }

  activeTabIsMessagesTab() {
    return this.activeTab.heading === 'Messages';
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }
}
