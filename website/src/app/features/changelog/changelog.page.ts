import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { PageHeader } from '@shared/ui/page-header';
import { RELEASES } from './changelog.data';

@Component({
  selector: 'rh-changelog-page',
  imports: [RouterLink, PageHeader, InlineMarkupPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Releases" icon="bi-clock-history" title="Changelog"
                    lead="What changed between versions. Breaking changes are always called out; see the migration guide when upgrading across majors." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="releases">
        @for (release of releases; track release.version; let first = $first) {
          <article class="release">
            <div class="version">
              <span class="v">v{{ release.version }}</span>
              @if (first) { <span class="latest">Latest</span> }
            </div>
            <div class="surface p-4 flex-grow-1 min-w-0">
              <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
                <h2 class="h5 fw-bold mb-0">{{ release.headline }}</h2>
                <a class="small fw-semibold" [routerLink]="['/changelog', release.version]">Release notes and upgrade steps <i class="bi bi-arrow-right"></i></a>
              </div>
              @for (group of release.groups; track group.kind) {
                <h3 class="kind" [class]="'kind k-' + group.kind.toLowerCase()">
                  {{ group.kind }} @if (group.breaking) { <span class="breaking">breaking</span> }
                </h3>
                <ul>
                  @for (item of group.items; track $index) { <li [innerHTML]="item | inlineMarkup"></li> }
                </ul>
              }
            </div>
          </article>
        }
      </div>
      <p class="text-muted-rh mt-4">Upgrading from v12? Read <a routerLink="/docs/migration-v13">Migrating to v13</a>. From v11? Read <a routerLink="/docs/migration-v12">Migrating to v12</a>.</p>
    </div>
  `,
  styles: `
    .release { display: flex; gap: 2rem; margin-bottom: 1.5rem; }
    .version { width: 7rem; flex-shrink: 0; padding-top: 1rem; text-align: right; }
    .v { display: block; font-family: var(--bs-font-monospace); font-weight: 800; font-size: 1.3rem; }
    .latest { font-size: .7rem; font-weight: 700; color: var(--rh-success); text-transform: uppercase; letter-spacing: .08em; }
    .kind { font-size: .75rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; margin: 1rem 0 .4rem; }
    .k-added { color: var(--rh-success); }
    .k-changed, .k-deprecated { color: var(--rh-warning); }
    .k-fixed { color: var(--rh-info); }
    .k-removed { color: var(--rh-danger); }
    .breaking { margin-left: .4rem; padding: .05rem .45rem; border-radius: 999px; background: color-mix(in srgb, var(--rh-danger) 14%, transparent); color: var(--rh-danger); }
    ul { padding-left: 1.2rem; line-height: 1.7; margin-bottom: 0; }
    @media (max-width: 767.98px) {
      .release { flex-direction: column; gap: .5rem; }
      .version { width: auto; text-align: left; padding-top: 0; display: flex; gap: .75rem; align-items: baseline; }
    }
  `,
})
export class ChangelogPage {
  protected readonly releases = RELEASES;
}
