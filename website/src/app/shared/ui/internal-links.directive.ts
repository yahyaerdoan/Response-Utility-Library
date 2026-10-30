import { Directive, inject } from '@angular/core';
import { Router } from '@angular/router';

@Directive({
  selector: '[rhInternalLinks]',
  host: { '(click)': 'onClick($event)' },
})
export class InternalLinksDirective {
  private readonly router = inject(Router);

  onClick(event: MouseEvent): void {
    const target = event.target;
    if (event.defaultPrevented || !(target instanceof Element) || event.ctrlKey || event.metaKey || event.button !== 0) {
      return;
    }
    const anchor = target.closest('a[href]');
    const href = anchor?.getAttribute('href');
    if (anchor && href && isInternal(href) && !anchor.hasAttribute('target')) {
      event.preventDefault();
      void this.router.navigateByUrl(href);
    }
  }
}

function isInternal(href: string): boolean {
  return href.startsWith('/') && !href.startsWith('//');
}
