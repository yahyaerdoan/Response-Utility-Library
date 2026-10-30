import { ChangeDetectionStrategy, Component, computed, effect, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { CodeLanguage } from '@core/domain/content.model';
import { highlight } from '@shared/util/highlighter';
import { CopyButton } from './copy-button';

const LANGUAGE_LABELS: Record<CodeLanguage, string> = {
  csharp: 'C#',
  json: 'JSON',
  bash: 'Terminal',
  xml: 'XML',
  typescript: 'TypeScript',
  powershell: 'PowerShell',
  http: 'HTTP',
  plaintext: 'Text',
};

const EXPANDABLE_LINES = 12;

@Component({
  selector: 'rh-code-block',
  imports: [CopyButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <figure class="code m-0" [class.flush]="flush()">
      @if (!flush()) {
        <figcaption class="code-head">
          <span class="lang">{{ title() ?? languageLabel() }}</span>
          <span class="tools">
            @if (expandable()) {
              <button type="button" class="tool" (click)="expanded.set(true)" aria-label="Expand code" title="Expand"><i class="bi bi-arrows-angle-expand"></i></button>
            }
            <rh-copy-button [text]="code()" />
          </span>
        </figcaption>
      } @else {
        <div class="floating-copy"><rh-copy-button [text]="code()" /></div>
      }
      <pre class="m-0"><code [innerHTML]="html()"></code></pre>
    </figure>

    @if (expanded()) {
      <dialog #viewer class="viewer" [attr.aria-label]="(title() ?? languageLabel()) + ' code'" (close)="expanded.set(false)" (click)="onViewerClick($event)">
        <div class="viewer-head">
          <span class="lang">{{ title() ?? languageLabel() }} · {{ lineCount() }} lines</span>
          <span class="tools">
            <rh-copy-button [text]="code()" />
            <button type="button" class="tool" (click)="expanded.set(false)" aria-label="Close"><i class="bi bi-x-lg"></i></button>
          </span>
        </div>
        <pre class="m-0 viewer-pre"><code [innerHTML]="html()"></code></pre>
      </dialog>
    }
  `,
  styles: `
    :host { display: block; min-width: 0; }
    .code { position: relative; background: var(--rh-code-bg); border: 1px solid var(--rh-border); border-radius: var(--bs-border-radius-lg); overflow: hidden; }
    .code.flush { border: 0; border-radius: 0; }
    .code-head, .viewer-head { display: flex; justify-content: space-between; align-items: center; padding: .4rem .5rem .4rem 1rem; border-bottom: 1px solid var(--rh-border); background: var(--rh-surface-2); }
    .lang { font-size: .75rem; font-weight: 600; color: var(--rh-muted); letter-spacing: .04em; }
    .tools { display: inline-flex; gap: .35rem; }
    .tool { color: var(--rh-muted); border: 1px solid var(--rh-border); background: var(--rh-surface); line-height: 1; padding: .3rem .45rem; border-radius: .375rem; font-size: .8rem; }
    .tool:hover { color: var(--rh-text); border-color: var(--rh-accent); }
    .floating-copy { position: absolute; top: .5rem; right: .5rem; opacity: 0; transition: opacity .15s; }
    .code:hover .floating-copy, .code:focus-within .floating-copy { opacity: 1; }
    pre { padding: 1rem 1.1rem; overflow-x: auto; font-size: .84rem; line-height: 1.6; color: var(--rh-code-text); tab-size: 4; }
    .viewer { width: min(72rem, calc(100vw - 2rem)); max-height: calc(100vh - 2rem); padding: 0; border: 1px solid var(--rh-border); border-radius: 1rem; background: var(--rh-code-bg); color: var(--rh-code-text); overflow: hidden; }
    .viewer[open] { display: flex; flex-direction: column; }
    .viewer::backdrop { background: rgba(5, 3, 15, .6); backdrop-filter: blur(3px); }
    .viewer-pre { flex: 1; overflow: auto; font-size: .95rem; padding: 1.25rem 1.5rem; }
    @media (max-width: 575.98px) {
      pre { white-space: pre-wrap; overflow-wrap: anywhere; font-size: .76rem; padding: .85rem .9rem; tab-size: 2; }
      .floating-copy { opacity: 1; }
      .viewer-pre { font-size: .8rem; padding: 1rem; }
    }
  `,
})
export class CodeBlock {
  private readonly sanitizer = inject(DomSanitizer);
  private readonly viewer = viewChild<ElementRef<HTMLDialogElement>>('viewer');

  readonly code = input.required<string>();
  readonly language = input<CodeLanguage>('csharp');
  readonly title = input<string>();
  readonly flush = input(false);

  protected readonly expanded = signal(false);
  protected readonly lineCount = computed(() => this.code().split('\n').length);
  protected readonly expandable = computed(() => this.lineCount() > EXPANDABLE_LINES);
  protected readonly languageLabel = computed(() => LANGUAGE_LABELS[this.language()]);
  protected readonly html = computed(() => this.sanitizer.bypassSecurityTrustHtml(highlight(this.code(), this.language())));

  constructor() {
    effect(() => {
      const dialog = this.viewer()?.nativeElement;
      if (dialog && !dialog.open) {
        dialog.showModal();
      }
    });
  }

  protected onViewerClick(event: MouseEvent): void {
    if (event.target === this.viewer()?.nativeElement) {
      this.expanded.set(false);
    }
  }
}
