import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { TabSyncService } from '@core/services/tab-sync.service';

const OPTIONS = [
  { label: 'Minimal API', icon: 'bi-lightning' },
  { label: 'MVC controller', icon: 'bi-diagram-2' },
] as const;

@Component({
  selector: 'rh-api-style-pivot',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="pivot" role="radiogroup" aria-label="API style">
      <span class="caption">API style</span>
      @for (option of options; track option.label) {
        <button type="button" role="radio" class="option" [class.active]="current() === option.label"
                [attr.aria-checked]="current() === option.label" (click)="select(option.label)">
          <i class="bi me-1" [class]="option.icon"></i>{{ option.label }}
        </button>
      }
    </div>
  `,
  styles: `
    .pivot { display: inline-flex; flex-wrap: wrap; align-items: center; gap: .25rem; padding: .25rem; border: 1px solid var(--rh-border); border-radius: 999px; background: var(--rh-surface-2); }
    .caption { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .08em; color: var(--rh-muted); padding: 0 .5rem 0 .6rem; }
    .option { border: 0; border-radius: 999px; background: transparent; color: var(--rh-muted); font-size: .82rem; font-weight: 600; padding: .3rem .8rem; }
    .option:hover { color: var(--rh-text); }
    .option.active { background: var(--rh-surface); color: var(--rh-text); box-shadow: 0 1px 3px rgba(0, 0, 0, .15); }
  `,
})
export class ApiStylePivot {
  private readonly sync = inject(TabSyncService);
  private readonly selection = this.sync.selection(TAB_GROUPS.apiStyle);

  protected readonly options = OPTIONS;
  protected readonly current = computed(() => this.selection() ?? OPTIONS[0].label);

  protected select(label: string): void {
    this.sync.select(TAB_GROUPS.apiStyle, label);
  }
}
