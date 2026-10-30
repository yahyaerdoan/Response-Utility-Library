import { afterNextRender, DestroyRef, ElementRef, inject, signal, Signal } from '@angular/core';

export function inView(threshold = 0.25): Signal<boolean> {
  const host = inject<ElementRef<HTMLElement>>(ElementRef);
  const destroyRef = inject(DestroyRef);
  const visible = signal(false);

  afterNextRender(() => {
    if (typeof IntersectionObserver === 'undefined') {
      visible.set(true);
      return;
    }
    const observer = new IntersectionObserver(([entry]) => visible.set(entry.isIntersecting), { threshold });
    observer.observe(host.nativeElement);
    destroyRef.onDestroy(() => observer.disconnect());
  });

  return visible.asReadonly();
}

export function prefersReducedMotion(): boolean {
  return typeof matchMedia === 'function' && matchMedia('(prefers-reduced-motion: reduce)').matches;
}
