import { Directive, DOCUMENT, inject, input } from '@angular/core';

@Directive({
  selector: 'a[rhFragmentLink]',
  host: {
    '[attr.href]': "'#' + rhFragmentLink()",
    '(click)': 'onClick($event)',
  },
})
export class FragmentLinkDirective {
  private readonly document = inject(DOCUMENT);

  readonly rhFragmentLink = input.required<string>();

  onClick(event: MouseEvent): void {
    if (event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0) {
      return;
    }
    const target = this.document.getElementById(this.rhFragmentLink());
    const view = this.document.defaultView;
    if (!target || !view) {
      return;
    }
    event.preventDefault();
    const reduceMotion = view.matchMedia('(prefers-reduced-motion: reduce)').matches;
    target.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'start' });
    view.history.replaceState(view.history.state, '', `${view.location.pathname}${view.location.search}#${this.rhFragmentLink()}`);
  }
}
