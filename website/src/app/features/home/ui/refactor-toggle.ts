import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';

const BEFORE = `private static async Task<IResult> Handle(Guid id, ISender sender, HttpContext httpContext)
{
    var currentUser = await sender.Send(new GetCurrentUserQuery());
    if (!currentUser.IsSuccessful || currentUser.Data is null)
    {
        return currentUser.ToEnvelopedResult(httpContext);
    }

    var result = await sender.Send(new DeleteUserAddressCommand(id, currentUser.Data.Id));
    return result.ToEnvelopedResult(httpContext);
}`;

const AFTER = `private static async Task<IResult> Handle(Guid id, ISender sender, HttpContext httpContext)
    => (await sender.Send(new GetCurrentUserQuery())
            .BindAsync(user => sender.Send(new DeleteUserAddressCommand(id, user.Id))))
        .ToEnvelopedResult(httpContext);`;

const bodyLines = (code: string): number => code.split('\n').length - 1;

@Component({
  selector: 'rh-refactor-toggle',
  imports: [RouterLink, CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-3">
      <div class="segmented" role="group" aria-label="Show the endpoint before or after">
        <button type="button" [class.active]="!showAfter()" [attr.aria-pressed]="!showAfter()" (click)="showAfter.set(false)">Before</button>
        <button type="button" [class.active]="showAfter()" [attr.aria-pressed]="showAfter()" (click)="showAfter.set(true)">After</button>
      </div>
      <span class="lines" aria-live="polite">
        <strong>{{ lines() }}</strong> lines in the method body
        @if (showAfter()) {
          <span class="saved">-{{ saved }}</span>
        }
      </span>
    </div>
    <rh-code-block [code]="code()" language="csharp" />
    <p class="small text-muted-rh mt-3 mb-0">
      Same behavior: a failed user lookup is returned unchanged and the command is never sent.
      From a <a routerLink="/docs/case-study">real project</a> where 30 endpoints looked like this.
    </p>
  `,
  styles: `
    :host { display: block; }
    .segmented { display: inline-flex; padding: .2rem; border: 1px solid var(--rh-border); border-radius: 999px; background: var(--rh-surface); }
    .segmented button { border: 0; background: transparent; color: var(--rh-muted); font-weight: 600; padding: .35rem 1.1rem; border-radius: 999px; }
    .segmented button.active { background: var(--rh-accent); color: #fff; }
    .lines { color: var(--rh-muted); font-size: .9rem; }
    .lines strong { color: var(--rh-text); font-variant-numeric: tabular-nums; }
    .saved { margin-left: .5rem; font-weight: 700; color: var(--rh-success); }
  `,
})
export class RefactorToggle {
  protected readonly showAfter = signal(false);
  protected readonly saved = bodyLines(BEFORE) - bodyLines(AFTER);
  protected readonly code = computed(() => (this.showAfter() ? AFTER : BEFORE));
  protected readonly lines = computed(() => bodyLines(this.code()));
}
