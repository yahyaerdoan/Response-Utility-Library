import { ChangeDetectionStrategy, Component, DOCUMENT, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { DocsRepository } from '../application/docs-repository';

const COLLAPSED_KEY = 'rh-docs-sidebar-collapsed';

@Component({
  selector: 'rh-docs-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:keydown.escape)': 'menuOpen.set(false)' },
  template: `
    <div class="mobile-bar d-lg-none">
      <div class="container-xxl">
        <button type="button" class="mobile-toggle" (click)="menuOpen.set(!menuOpen())"
                [attr.aria-expanded]="menuOpen()" aria-controls="docs-nav">
          <i class="bi bi-list"></i>
          <span class="flex-grow-1 text-start text-truncate">{{ currentTitle() }}</span>
          <i class="bi chevron" [class.bi-chevron-down]="!menuOpen()" [class.bi-chevron-up]="menuOpen()"></i>
        </button>
      </div>
    </div>
    @if (menuOpen()) {
      <div class="backdrop d-lg-none" (click)="menuOpen.set(false)"></div>
    }
    <div class="container-xxl">
      <div class="docs-grid" [class.collapsed]="collapsed()">
        @if (collapsed()) {
          <button type="button" class="expand-btn d-none d-lg-grid" (click)="setCollapsed(false)" aria-label="Expand sidebar" title="Expand sidebar">
            <i class="bi bi-layout-sidebar"></i>
          </button>
        }
        <nav id="docs-nav" class="sidebar" [class.open]="menuOpen()" [class.d-lg-none]="collapsed()" aria-label="Documentation">
          <button type="button" class="collapse-btn d-none d-lg-flex" (click)="setCollapsed(true)" aria-label="Collapse sidebar">
            <i class="bi bi-layout-sidebar-inset"></i>Collapse sidebar
          </button>
          @for (group of groups; track group.title) {
            <div class="group">
              <h2 class="group-title">{{ group.title }}</h2>
              <ul class="list-unstyled mb-0">
                @for (page of group.pages; track page.slug) {
                  <li>
                    <a [routerLink]="['/docs', page.slug]" routerLinkActive="active" class="nav-item">
                      <i class="bi" [class]="page.icon"></i>
                      <span class="flex-grow-1">{{ page.title }}</span>
                      @if (page.badge; as badge) { <span class="mini-badge" [class]="'mini-badge tone-' + badge.tone">{{ badge.label }}</span> }
                    </a>
                  </li>
                }
              </ul>
            </div>
          }
        </nav>
        <div class="content"><router-outlet /></div>
      </div>
    </div>
  `,
  styles: `
    .docs-grid { display: grid; grid-template-columns: 17.5rem minmax(0, 1fr); gap: 2.5rem; }
    .sidebar { position: sticky; top: 4rem; align-self: start; max-height: calc(100vh - 4rem); overflow-y: auto; overscroll-behavior: contain; scrollbar-width: thin; padding: 2rem .25rem 2rem 0; border-right: 1px solid var(--rh-border); }
    .group { margin-bottom: 1.5rem; }
    .group-title { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .1em; color: var(--rh-muted); margin: 0 0 .5rem .75rem; }
    .nav-item { display: flex; align-items: center; gap: .6rem; padding: .38rem .75rem; margin-right: .75rem; border-radius: .5rem; color: var(--rh-text); font-size: .9rem; }
    .nav-item .bi { color: var(--rh-muted); }
    .nav-item:hover { background: var(--rh-surface-2); }
    .nav-item.active { background: var(--rh-accent-soft); color: var(--rh-accent); font-weight: 600; }
    .nav-item.active .bi { color: var(--rh-accent); }
    .mini-badge { font-size: .62rem; font-weight: 700; padding: .1rem .4rem; border-radius: 999px; white-space: nowrap; }
    .tone-accent { background: var(--rh-accent-soft); color: var(--rh-accent); }
    .tone-new { background: color-mix(in srgb, var(--rh-success) 15%, transparent); color: var(--rh-success); }
    .tone-muted { background: var(--rh-surface-2); color: var(--rh-muted); }
    .collapse-btn { display: flex; align-items: center; gap: .5rem; width: calc(100% - .75rem); margin-bottom: 1rem; padding: .35rem .75rem; border: 1px dashed var(--rh-border); border-radius: .5rem; background: transparent; color: var(--rh-muted); font-size: .8rem; }
    .collapse-btn:hover { color: var(--rh-text); border-color: var(--rh-accent); }
    .expand-btn { position: sticky; top: 5rem; align-self: start; margin-top: 2rem; width: 2.25rem; height: 2.25rem; display: grid; place-items: center; border: 1px solid var(--rh-border); border-radius: .6rem; background: var(--rh-surface); color: var(--rh-text); }
    .expand-btn:hover { border-color: var(--rh-accent); color: var(--rh-accent); }
    .docs-grid.collapsed { grid-template-columns: 2.25rem minmax(0, 1fr); }
    .content { min-width: 0; }

    .mobile-bar { position: sticky; top: 4rem; z-index: 1020; background: var(--rh-header-bg); backdrop-filter: saturate(160%) blur(14px); border-bottom: 1px solid var(--rh-border); }
    .mobile-toggle { display: flex; align-items: center; gap: .6rem; width: 100%; padding: .7rem 0; border: 0; background: transparent; color: var(--rh-text); font-weight: 600; font-size: .92rem; }
    .mobile-toggle .chevron { color: var(--rh-muted); }
    .backdrop { position: fixed; inset: 0; z-index: 1015; background: rgba(5, 3, 15, .45); }

    @media (max-width: 991.98px) {
      .docs-grid, .docs-grid.collapsed { grid-template-columns: minmax(0, 1fr); gap: 0; }
      .sidebar {
        display: none;
        position: fixed;
        top: 6.9rem;
        left: 0;
        right: 0;
        z-index: 1019;
        max-height: calc(100vh - 7.5rem);
        padding: 1rem;
        border-right: 0;
        border-bottom: 1px solid var(--rh-border);
        background: var(--rh-bg);
        box-shadow: 0 1rem 2rem -1rem rgba(0, 0, 0, .5);
      }
      .sidebar.open { display: block; }
      .nav-item { margin-right: 0; padding: .55rem .75rem; }
    }
  `,
})
export class DocsLayout {
  private readonly repository = inject(DocsRepository);
  private readonly router = inject(Router);
  private readonly document = inject(DOCUMENT);

  protected readonly groups = this.repository.groups();
  protected readonly menuOpen = signal(false);
  protected readonly collapsed = signal(this.readCollapsed());
  protected readonly currentTitle = signal(this.titleFor(this.router.url));

  constructor() {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(event => {
        this.menuOpen.set(false);
        this.currentTitle.set(this.titleFor(event.urlAfterRedirects));
      });
  }

  protected setCollapsed(value: boolean): void {
    this.collapsed.set(value);
    try {
      this.document.defaultView?.localStorage.setItem(COLLAPSED_KEY, value ? '1' : '0');
    } catch {
      return;
    }
  }

  private readCollapsed(): boolean {
    try {
      return this.document.defaultView?.localStorage.getItem(COLLAPSED_KEY) === '1';
    } catch {
      return false;
    }
  }

  private titleFor(url: string): string {
    const slug = url.split(/[?#]/)[0].split('/').pop() ?? '';
    return this.repository.find(slug)?.title ?? 'Documentation';
  }
}
