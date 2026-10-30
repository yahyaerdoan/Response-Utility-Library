import { ChangeDetectionStrategy, Component, DOCUMENT, inject, signal } from '@angular/core';

const THRESHOLD = 600;

@Component({
  selector: 'rh-scroll-top',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(window:scroll)': 'onScroll()' },
  template: `
    <button type="button" class="to-top" [class.visible]="visible()" [attr.tabindex]="visible() ? 0 : -1"
            [attr.aria-hidden]="!visible()" aria-label="Back to top" (click)="toTop()">
      <i class="bi bi-arrow-up"></i>
    </button>
  `,
  styles: `
    .to-top { position: fixed; right: 1rem; bottom: 1rem; z-index: 1025; width: 2.75rem; height: 2.75rem; display: grid; place-items: center; border: 1px solid var(--rh-border); border-radius: 50%; background: var(--rh-surface); color: var(--rh-text); box-shadow: 0 .75rem 1.5rem -.75rem rgba(0, 0, 0, .45); opacity: 0; transform: translateY(.75rem); pointer-events: none; transition: opacity .2s ease, transform .2s ease, border-color .2s ease; }
    .to-top.visible { opacity: 1; transform: none; pointer-events: auto; }
    .to-top:hover { border-color: var(--rh-accent); color: var(--rh-accent); }
    @media (max-width: 991.98px) { .to-top { bottom: calc(1rem + var(--rh-fixed-bottom, 0px)); } }
  `,
})
export class ScrollTop {
  private readonly document = inject(DOCUMENT);
  protected readonly visible = signal(false);

  protected onScroll(): void {
    this.visible.set((this.document.defaultView?.scrollY ?? 0) > THRESHOLD);
  }

  protected toTop(): void {
    const view = this.document.defaultView;
    const reduce = view?.matchMedia('(prefers-reduced-motion: reduce)').matches ?? false;
    view?.scrollTo({ top: 0, behavior: reduce ? 'auto' : 'smooth' });
    this.document.getElementById('main')?.focus({ preventScroll: true });
  }
}
