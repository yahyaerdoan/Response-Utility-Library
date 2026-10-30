import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { CodeSnippet } from '@core/domain/content.model';
import { TabSyncService } from '@core/services/tab-sync.service';
import { CodeBlock } from './code-block';

@Component({
  selector: 'rh-code-tabs',
  imports: [CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="tabs-shell">
      <div class="tab-bar" role="tablist">
        @for (snippet of snippets(); track $index) {
          <button type="button" role="tab" class="tab" [class.active]="$index === activeIndex()"
                  [attr.aria-selected]="$index === activeIndex()" (click)="choose($index)">
            {{ snippet.label ?? snippet.language }}
          </button>
        }
        @if (syncKey()) {
          <span class="synced" title="Your choice is remembered across the site"><i class="bi bi-link-45deg"></i></span>
        }
      </div>
      @if (active(); as snippet) {
        <rh-code-block [code]="snippet.code" [language]="snippet.language" [flush]="true" />
      }
    </div>
  `,
  styles: `
    :host { display: block; min-width: 0; }
    .tabs-shell { border: 1px solid var(--rh-border); border-radius: var(--bs-border-radius-lg); overflow: hidden; background: var(--rh-code-bg); }
    .tab-bar { display: flex; align-items: center; gap: .25rem; padding: .35rem .5rem 0; background: var(--rh-surface-2); border-bottom: 1px solid var(--rh-border); overflow-x: auto; scrollbar-width: none; }
    .tab-bar::-webkit-scrollbar { display: none; }
    .tab { border: 0; background: transparent; color: var(--rh-muted); font-size: .8rem; font-weight: 600; padding: .45rem .8rem; border-bottom: 2px solid transparent; white-space: nowrap; }
    .tab:hover { color: var(--rh-text); }
    .tab.active { color: var(--rh-text); border-bottom-color: var(--rh-accent); }
    .synced { margin-left: auto; padding: 0 .4rem .3rem; color: var(--rh-muted); font-size: .9rem; }
  `,
})
export class CodeTabs {
  private readonly sync = inject(TabSyncService);

  readonly snippets = input.required<readonly CodeSnippet[]>();
  readonly syncKey = input<string>();

  private readonly local = signal(0);
  private readonly shared = computed(() => {
    const key = this.syncKey();
    return key ? this.sync.selection(key)() : null;
  });

  protected readonly activeIndex = computed(() => {
    const snippets = this.snippets();
    const label = this.shared();
    const synced = label ? snippets.findIndex(snippet => snippet.label === label) : -1;
    return synced >= 0 ? synced : Math.min(this.local(), snippets.length - 1);
  });
  protected readonly active = computed(() => this.snippets()[this.activeIndex()]);

  protected choose(index: number): void {
    this.local.set(index);
    const key = this.syncKey();
    const label = this.snippets()[index]?.label;
    if (key && label) {
      this.sync.select(key, label);
    }
  }
}
