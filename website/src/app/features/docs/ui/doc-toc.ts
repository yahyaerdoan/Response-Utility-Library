import { afterRenderEffect, ChangeDetectionStrategy, Component, DestroyRef, DOCUMENT, inject, input, signal } from '@angular/core';
import { FragmentLinkDirective } from '@shared/ui/fragment-link.directive';
import { ContentSection } from '@core/domain/content.model';

@Component({
  selector: 'rh-doc-toc',
  imports: [FragmentLinkDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="toc" aria-label="On this page">
      <h2 class="toc-title">On this page</h2>
      <ul class="list-unstyled mb-0">
        @for (section of sections(); track section.id) {
          <li>
            <a [rhFragmentLink]="section.id" [class.active]="active() === section.id" (click)="active.set(section.id)">{{ section.title }}</a>
          </li>
        }
      </ul>
    </nav>
  `,
  styles: `
    .toc { position: sticky; top: 5.5rem; font-size: .85rem; }
    .toc-title { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .1em; color: var(--rh-muted); }
    ul { border-left: 1px solid var(--rh-border); }
    a { display: block; padding: .25rem 0 .25rem .9rem; margin-left: -1px; color: var(--rh-muted); border-left: 2px solid transparent; }
    a:hover { color: var(--rh-text); }
    a.active { color: var(--rh-accent); border-left-color: var(--rh-accent); font-weight: 600; }
  `,
})
export class DocToc {
  private readonly document = inject(DOCUMENT);
  readonly sections = input.required<readonly ContentSection[]>();
  protected readonly active = signal<string | null>(null);

  constructor() {
    const destroyRef = inject(DestroyRef);
    let observer: IntersectionObserver | null = null;
    destroyRef.onDestroy(() => observer?.disconnect());

    afterRenderEffect(() => {
      const ids = this.sections().map(section => section.id);
      observer?.disconnect();
      const view = this.document.defaultView;
      if (!view || !('IntersectionObserver' in view)) {
        return;
      }
      this.active.set(ids[0] ?? null);
      observer = new IntersectionObserver(
        entries => {
          const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
          if (visible[0]) {
            this.active.set(visible[0].target.id);
          }
        },
        { rootMargin: '-80px 0px -65% 0px' },
      );
      for (const id of ids) {
        const element = this.document.getElementById(id);
        if (element) {
          observer.observe(element);
        }
      }
    });
  }
}
