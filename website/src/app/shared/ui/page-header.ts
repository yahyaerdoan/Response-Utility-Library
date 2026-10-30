import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'rh-page-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-header bg-glow">
      <div class="container-xxl py-5">
        <span class="eyebrow"><i class="bi me-1" [class]="icon()"></i>{{ eyebrow() }}</span>
        <h1 class="display-6 fw-bold mt-2 mb-3">{{ title() }}</h1>
        <p class="lead text-muted-rh mb-0">{{ lead() }}</p>
        <ng-content />
      </div>
    </header>
  `,
  styles: `
    .page-header { border-bottom: 1px solid var(--rh-border); }
    .lead { max-width: 48rem; }
    @media (max-width: 575.98px) {
      .container-xxl { padding-top: 2rem !important; padding-bottom: 2rem !important; }
      .lead { font-size: 1rem; }
    }
  `,
})
export class PageHeader {
  readonly eyebrow = input.required<string>();
  readonly icon = input('bi-stars');
  readonly title = input.required<string>();
  readonly lead = input.required<string>();
}
