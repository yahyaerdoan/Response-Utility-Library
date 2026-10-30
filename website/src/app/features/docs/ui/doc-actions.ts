import { ChangeDetectionStrategy, Component, computed, DOCUMENT, inject, input, signal } from '@angular/core';
import { Clipboard } from '@core/ports/clipboard';
import { Dropdown } from '@shared/ui/dropdown';
import { MarkdownExporter } from '../application/markdown-exporter';
import { AI_TARGETS, aiPrompt, SHARE_TARGETS } from '../application/share-targets';
import { DocPage } from '../domain/doc-page';

type Feedback = 'markdown' | 'link' | null;

@Component({
  selector: 'rh-doc-actions',
  imports: [Dropdown],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="actions">
      <div class="split">
        <button type="button" class="primary" (click)="copyMarkdown()">
          <i class="bi" [class]="feedback() === 'markdown' ? 'bi-check2' : 'bi-markdown'"></i>
          {{ feedback() === 'markdown' ? 'Copied' : 'Copy page' }}
        </button>
        <rh-dropdown label="" ariaLabel="More page actions" icon="bi-three-dots-vertical" align="end">
          @for (target of aiTargets; track target.label) {
            <a role="menuitem" class="item" [href]="target.href(prompt())" target="_blank" rel="noopener">
              <i class="bi" [class]="target.icon"></i><span>{{ target.label }}</span><i class="bi bi-box-arrow-up-right ext"></i>
            </a>
          }
          <button type="button" role="menuitem" class="item" (click)="viewMarkdown()">
            <i class="bi bi-filetype-md"></i><span>View as Markdown</span>
          </button>
        </rh-dropdown>
      </div>

      <rh-dropdown label="Share" icon="bi-share" align="end">
        <button type="button" role="menuitem" class="item" (click)="copyLink()">
          <i class="bi" [class]="feedback() === 'link' ? 'bi-check2' : 'bi-link-45deg'"></i><span>{{ feedback() === 'link' ? 'Link copied' : 'Copy link' }}</span>
        </button>
        @if (canShareNatively) {
          <button type="button" role="menuitem" class="item" (click)="shareNatively()">
            <i class="bi bi-phone"></i><span>Share via device…</span>
          </button>
        }
        <hr>
        @for (target of shareTargets; track target.label) {
          <a role="menuitem" class="item" [href]="target.href(url(), page().title)" target="_blank" rel="noopener">
            <i class="bi" [class]="target.icon"></i><span>{{ target.label }}</span>
          </a>
        }
      </rh-dropdown>
    </div>
  `,
  styles: `
    .actions { display: flex; flex-wrap: wrap; gap: .5rem; }
    .split { display: inline-flex; }
    .primary { display: inline-flex; align-items: center; gap: .4rem; border: 1px solid var(--rh-border); border-right: 0; border-radius: .55rem 0 0 .55rem; background: var(--rh-surface); color: var(--rh-text); font-size: .82rem; font-weight: 600; padding: .35rem .7rem; }
    .primary:hover { border-color: var(--rh-accent); }
    .split ::ng-deep .trigger { border-radius: 0 .55rem .55rem 0; height: 100%; }
    .split ::ng-deep .trigger .caret { display: none; }
    .item { display: flex; align-items: center; gap: .6rem; width: 100%; padding: .45rem .6rem; border: 0; border-radius: .5rem; background: transparent; color: var(--rh-text); font-size: .85rem; text-align: left; }
    .item:hover, .item:focus-visible { background: var(--rh-surface-2); }
    .item span { flex: 1; }
    .item .bi { width: 1rem; color: var(--rh-muted); }
    .ext { font-size: .7rem; }
    hr { margin: .35rem 0; border-color: var(--rh-border); opacity: 1; }
  `,
})
export class DocActions {
  private readonly clipboard = inject(Clipboard);
  private readonly document = inject(DOCUMENT);
  private readonly exporter = new MarkdownExporter();

  readonly page = input.required<DocPage>();

  protected readonly aiTargets = AI_TARGETS;
  protected readonly shareTargets = SHARE_TARGETS;
  protected readonly feedback = signal<Feedback>(null);
  protected readonly canShareNatively = typeof navigator !== 'undefined' && typeof navigator.share === 'function';
  protected readonly url = computed(() => {
    const view = this.document.defaultView;
    return view ? `${view.location.origin}/docs/${this.page().slug}` : `/docs/${this.page().slug}`;
  });
  protected readonly prompt = computed(() => aiPrompt(this.url(), this.page().title));
  private readonly markdown = computed(() => this.exporter.export(this.page(), this.url()));

  protected async copyMarkdown(): Promise<void> {
    if (await this.clipboard.copy(this.markdown())) {
      this.flash('markdown');
    }
  }

  protected async copyLink(): Promise<void> {
    if (await this.clipboard.copy(this.url())) {
      this.flash('link');
    }
  }

  protected viewMarkdown(): void {
    const view = this.document.defaultView;
    if (!view) {
      return;
    }
    const blob = new Blob([this.markdown()], { type: 'text/plain;charset=utf-8' });
    const objectUrl = view.URL.createObjectURL(blob);
    view.open(objectUrl, '_blank', 'noopener');
    view.setTimeout(() => view.URL.revokeObjectURL(objectUrl), 60_000);
  }

  protected async shareNatively(): Promise<void> {
    try {
      await navigator.share({ title: this.page().title, text: this.page().summary, url: this.url() });
    } catch {
      return;
    }
  }

  private flash(kind: Feedback): void {
    this.feedback.set(kind);
    setTimeout(() => this.feedback.set(null), 1600);
  }
}
