import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { PageHeader } from '@shared/ui/page-header';
import { GEM_AREAS, GemArea, GEMS, ROADMAP_IDEAS } from './gems.data';

@Component({
  selector: 'rh-gems-page',
  imports: [RouterLink, PageHeader, CodeBlock, InlineMarkupPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Did you know?" icon="bi-gem" title="Hidden gems"
                    lead="Small design decisions that make a big difference in production: the things most people only discover after months of use." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="chip-row mb-4" role="group" aria-label="Filter by area">
        <button type="button" class="chip" [class.active]="area() === null" (click)="area.set(null)">All {{ total }}</button>
        @for (item of areas; track item) {
          <button type="button" class="chip" [class.active]="area() === item" (click)="area.set(item)">{{ item }}</button>
        }
      </div>

      <div class="masonry">
        @for (gem of visible(); track gem.id) {
          <article class="gem surface surface-hover">
            <div class="d-flex align-items-start gap-3">
              <span class="icon-tile"><i class="bi" [class]="gem.icon"></i></span>
              <div class="min-w-0">
                <span class="area">{{ gem.area }}</span>
                <h2 class="h6 fw-bold mb-2">{{ gem.title }}</h2>
                <p class="small mb-0 text-muted-rh" [innerHTML]="gem.text | inlineMarkup"></p>
              </div>
            </div>
            @if (gem.snippet) {
              <rh-code-block class="mt-3" [code]="gem.snippet" language="csharp" [flush]="true" />
            }
            @if (gem.link) {
              <a class="more small" [routerLink]="linkPath(gem.link)" [queryParams]="linkQuery(gem.link)">Learn more<i class="bi bi-arrow-right ms-1"></i></a>
            }
          </article>
        }
      </div>

      <section class="mt-5 pt-4" aria-labelledby="ideas">
        <span class="eyebrow">Ideas, not features</span>
        <h2 id="ideas" class="h3 fw-bold mt-1">Where it could go next</h2>
        <p class="text-muted-rh">These are <strong>not</strong> in the library today. They are natural extensions of its design, collected as roadmap ideas.</p>
        <div class="row g-3">
          @for (idea of ideas; track idea.title) {
            <div class="col-md-6 col-xl-4">
              <div class="idea h-100 p-3">
                <i class="bi me-2" [class]="idea.icon"></i><strong>{{ idea.title }}</strong>
                <p class="small text-muted-rh mb-0 mt-1" [innerHTML]="idea.text | inlineMarkup"></p>
              </div>
            </div>
          }
        </div>
      </section>
    </div>
  `,
  styles: `
    .masonry { columns: 3 20rem; column-gap: 1rem; }
    .gem { break-inside: avoid; margin-bottom: 1rem; padding: 1.25rem; overflow: hidden; }
    .gem rh-code-block { display: block; margin: 0 -1.25rem; border-top: 1px solid var(--rh-border); border-bottom: 1px solid var(--rh-border); }
    .area { font-size: .68rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--rh-accent); }
    .more { display: inline-block; margin-top: .9rem; font-weight: 600; }
    .idea { border: 1px dashed var(--rh-border); border-radius: var(--bs-border-radius-lg); }
    .idea .bi { color: var(--rh-accent); }
    @media (max-width: 575.98px) { .gem { padding: 1rem; } .gem rh-code-block { margin: 0 -1rem; } }
  `,
})
export class GemsPage {
  protected readonly areas = GEM_AREAS;
  protected readonly ideas = ROADMAP_IDEAS;
  protected readonly total = GEMS.length;
  protected readonly area = signal<GemArea | null>(null);
  protected readonly visible = computed(() => {
    const area = this.area();
    return area ? GEMS.filter(gem => gem.area === area) : GEMS;
  });

  protected linkPath(link: string): string {
    return link.split('?')[0];
  }

  protected linkQuery(link: string): Record<string, string> {
    return Object.fromEntries(new URLSearchParams(link.split('?')[1] ?? ''));
  }
}
