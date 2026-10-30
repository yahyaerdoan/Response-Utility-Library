import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { InternalLinksDirective } from '@shared/ui/internal-links.directive';
import { CHANGE_ICONS, findRelease, RELEASES } from './changelog.data';

@Component({
  selector: 'rh-release-page',
  imports: [RouterLink, InlineMarkupPipe],
  hostDirectives: [InternalLinksDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (release(); as current) {
      <header class="release-hero bg-glow">
        <div class="container-xxl py-5">
          <a routerLink="/changelog" class="small"><i class="bi bi-arrow-left me-1"></i>All releases</a>
          <span class="eyebrow d-block mt-3">What's new</span>
          <h1 class="display-6 fw-bold mt-1">ResponseResultHandler {{ current.version }}</h1>
          <p class="lead text-muted-rh mb-0">{{ current.summary }}</p>
        </div>
      </header>

      <div class="container-xxl py-4 py-lg-5">
        <div class="row g-4">
          <div class="col-lg-8">
            <section class="upgrade surface p-4 mb-4" aria-labelledby="upgrade">
              <h2 id="upgrade" class="h5 fw-bold"><i class="bi bi-arrow-up-circle me-2"></i>Upgrade to {{ current.version }}</h2>
              <ol class="mb-0">
                @for (item of current.upgrade; track $index) { <li [innerHTML]="item | inlineMarkup"></li> }
              </ol>
            </section>

            @for (group of current.groups; track group.kind) {
              <section class="group mb-4">
                <h2 class="h5 fw-bold" [class]="'h5 fw-bold kind-' + group.kind.toLowerCase()">
                  <i class="bi me-2" [class]="icons[group.kind]"></i>{{ group.kind }}
                  @if (group.breaking) { <span class="breaking">breaking</span> }
                </h2>
                <ul>
                  @for (item of group.items; track $index) { <li [innerHTML]="item | inlineMarkup"></li> }
                </ul>
              </section>
            }
          </div>
          <aside class="col-lg-4">
            <div class="surface p-3 side">
              <h2 class="label">Other releases</h2>
              <ul class="list-unstyled mb-0">
                @for (other of releases; track other.version) {
                  <li>
                    <a [routerLink]="['/changelog', other.version]" [class.active]="other.version === current.version">
                      <span class="v">v{{ other.version }}</span><span class="text-muted-rh small">{{ other.headline }}</span>
                    </a>
                  </li>
                }
              </ul>
            </div>
          </aside>
        </div>
      </div>
    } @else {
      <div class="container py-5 text-center">
        <h1 class="h3">Release not found</h1>
        <a routerLink="/changelog" class="btn btn-accent mt-3">All releases</a>
      </div>
    }
  `,
  styles: `
    .release-hero { border-bottom: 1px solid var(--rh-border); }
    .upgrade { border-color: color-mix(in srgb, var(--rh-accent) 45%, var(--rh-border)); }
    .upgrade ol, .group ul { line-height: 1.8; padding-left: 1.25rem; }
    .kind-added { color: var(--rh-success); }
    .kind-changed, .kind-deprecated { color: var(--rh-warning); }
    .kind-fixed { color: var(--rh-info); }
    .kind-removed { color: var(--rh-danger); }
    .breaking { margin-left: .5rem; font-size: .7rem; text-transform: uppercase; letter-spacing: .06em; padding: .1rem .5rem; border-radius: 999px; background: color-mix(in srgb, var(--rh-danger) 14%, transparent); color: var(--rh-danger); }
    .side { position: sticky; top: 5rem; }
    .label { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--rh-muted); }
    .side a { display: flex; flex-direction: column; padding: .5rem .6rem; border-radius: .5rem; color: var(--rh-text); }
    .side a:hover { background: var(--rh-surface-2); }
    .side a.active { background: var(--rh-accent-soft); }
    .v { font-family: var(--bs-font-monospace); font-weight: 700; }
  `,
})
export class ReleasePage {
  private readonly titleService = inject(Title);

  readonly version = input.required<string>();

  protected readonly icons = CHANGE_ICONS;
  protected readonly releases = RELEASES;
  protected readonly release = computed(() => findRelease(this.version()));

  constructor() {
    effect(() => this.titleService.setTitle(`What's new in ${this.version()} · ResponseResultHandler`));
  }
}
