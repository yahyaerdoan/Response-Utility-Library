import { ChangeDetectionStrategy, Component, DOCUMENT, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ANNOUNCEMENT } from '@core/navigation';

const STORAGE_PREFIX = 'rh-banner-dismissed:';

@Component({
  selector: 'rh-announcement-banner',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (visible()) {
      <div class="banner" role="region" aria-label="Announcement">
        <div class="container-xxl inner">
          <a class="message" [routerLink]="announcement.link">
            <span class="spark" aria-hidden="true">✨</span>
            <span>{{ announcement.text }}</span>
            <span class="cta">{{ announcement.cta }} <i class="bi bi-arrow-right"></i></span>
          </a>
          <button type="button" class="dismiss" (click)="dismiss()" aria-label="Dismiss announcement"><i class="bi bi-x-lg"></i></button>
        </div>
      </div>
    }
  `,
  styles: `
    .banner { background: linear-gradient(100deg, var(--rh-accent), var(--rh-accent-2)); color: #fff; font-size: .85rem; }
    .inner { display: flex; align-items: center; justify-content: center; gap: .75rem; min-height: 2.4rem; position: relative; padding-right: 2.5rem; }
    .message { display: inline-flex; flex-wrap: wrap; align-items: center; justify-content: center; gap: .4rem; color: #fff; text-align: center; padding: .35rem 0; }
    .message:hover .cta { text-decoration: underline; }
    .cta { font-weight: 700; white-space: nowrap; }
    .dismiss { position: absolute; right: .5rem; border: 0; background: transparent; color: #fff; opacity: .85; padding: .25rem .5rem; border-radius: .4rem; }
    .dismiss:hover { opacity: 1; background: rgba(255, 255, 255, .15); }
  `,
})
export class AnnouncementBanner {
  private readonly document = inject(DOCUMENT);

  protected readonly announcement = ANNOUNCEMENT;
  protected readonly visible = signal(!this.isDismissed());

  protected dismiss(): void {
    this.visible.set(false);
    try {
      this.document.defaultView?.localStorage.setItem(STORAGE_PREFIX + ANNOUNCEMENT.id, '1');
    } catch {
      return;
    }
  }

  private isDismissed(): boolean {
    try {
      return this.document.defaultView?.localStorage.getItem(STORAGE_PREFIX + ANNOUNCEMENT.id) === '1';
    } catch {
      return false;
    }
  }
}
