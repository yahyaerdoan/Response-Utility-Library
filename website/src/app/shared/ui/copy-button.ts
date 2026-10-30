import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { Clipboard } from '@core/ports/clipboard';

@Component({
  selector: 'rh-copy-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button type="button" class="btn btn-sm copy" [class.done]="copied()" (click)="copy()"
            [attr.aria-label]="copied() ? 'Copied' : 'Copy to clipboard'" [title]="copied() ? 'Copied' : 'Copy'">
      <i class="bi" [class.bi-clipboard]="!copied()" [class.bi-check2]="copied()"></i>
    </button>
  `,
  styles: `
    .copy { color: var(--rh-muted); border: 1px solid var(--rh-border); background: var(--rh-surface); line-height: 1; padding: .3rem .45rem; }
    .copy:hover { color: var(--rh-text); border-color: var(--rh-accent); }
    .copy.done { color: var(--rh-success); border-color: var(--rh-success); }
  `,
})
export class CopyButton {
  private readonly clipboard = inject(Clipboard);
  readonly text = input.required<string>();
  protected readonly copied = signal(false);

  protected async copy(): Promise<void> {
    if (await this.clipboard.copy(this.text())) {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 1600);
    }
  }
}
