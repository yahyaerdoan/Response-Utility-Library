import { inject } from '@angular/core';
import { IsActiveMatchOptions, Router, ViewTransitionInfo } from '@angular/router';

const SAME_PAGE: IsActiveMatchOptions = {
  paths: 'exact',
  matrixParams: 'exact',
  queryParams: 'ignored',
  fragment: 'ignored',
};

export function skipSamePageTransitions({ transition }: ViewTransitionInfo): void {
  const router = inject(Router);
  const target = router.currentNavigation()?.finalUrl;
  if (target && router.isActive(target, SAME_PAGE)) {
    transition.skipTransition();
  }
}
