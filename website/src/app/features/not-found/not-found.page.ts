import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';

@Component({
  selector: 'rh-not-found-page',
  imports: [RouterLink, CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container py-5 text-center bg-glow">
      <p class="display-1 fw-bold text-gradient mb-0">404</p>
      <h1 class="h3 fw-bold">Result.NotFound()</h1>
      <p class="text-muted-rh">This page does not exist, but at least it failed explicitly.</p>
      <rh-code-block class="d-block mx-auto my-4 text-start problem" [code]="problem" language="json" title="application/problem+json" />
      <a routerLink="/" class="btn btn-accent">Back home</a>
    </div>
  `,
  styles: `.problem { max-width: 32rem; }`,
})
export class NotFoundPage {
  protected readonly problem = JSON.stringify(
    {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
      title: 'Not Found',
      status: 404,
      detail: 'The page you are looking for does not exist.',
      instance: typeof location === 'undefined' ? '/' : location.pathname,
    },
    null,
    2,
  );
}
