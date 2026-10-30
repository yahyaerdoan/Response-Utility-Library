import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { KeyboardService, KeyboardStyle } from '@core/services/keyboard.service';
import { ThemePreference, ThemeService } from '@core/services/theme.service';

const THEMES: readonly { value: ThemePreference; label: string; icon: string }[] = [
  { value: 'light', label: 'Light', icon: 'bi-sun' },
  { value: 'auto', label: 'Auto', icon: 'bi-circle-half' },
  { value: 'dark', label: 'Dark', icon: 'bi-moon-stars' },
];

const KEYBOARDS: readonly { value: KeyboardStyle; label: string }[] = [
  { value: 'auto', label: 'Auto-detect' },
  { value: 'mac', label: 'macOS (⌘)' },
  { value: 'windows', label: 'Windows / Linux (Ctrl)' },
];

@Component({
  selector: 'rh-preferences',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="prefs">
      <div class="segmented" role="radiogroup" aria-label="Theme">
        @for (item of themes; track item.value) {
          <button type="button" role="radio" [class.active]="theme.preference() === item.value" [attr.aria-checked]="theme.preference() === item.value"
                  (click)="theme.setPreference(item.value)" [title]="item.label">
            <i class="bi" [class]="item.icon"></i><span>{{ item.label }}</span>
          </button>
        }
      </div>
      <label class="kbd-select">
        <i class="bi bi-keyboard"></i>
        <span class="visually-hidden">Keyboard shortcuts style</span>
        <select class="form-select form-select-sm" [value]="keyboard.style()" (change)="setKeyboard($event)">
          @for (item of keyboards; track item.value) {
            <option [value]="item.value" [selected]="keyboard.style() === item.value">{{ item.label }}</option>
          }
        </select>
      </label>
    </div>
  `,
  styles: `
    .prefs { display: flex; flex-wrap: wrap; align-items: center; gap: .75rem; }
    .segmented { display: inline-flex; padding: .2rem; border: 1px solid var(--rh-border); border-radius: 999px; background: var(--rh-surface); }
    .segmented button { display: inline-flex; align-items: center; gap: .35rem; border: 0; border-radius: 999px; background: transparent; color: var(--rh-muted); font-size: .78rem; font-weight: 600; padding: .25rem .7rem; }
    .segmented button.active { background: var(--rh-accent); color: #fff; }
    .kbd-select { display: inline-flex; align-items: center; gap: .45rem; color: var(--rh-muted); }
    .kbd-select select { width: auto; border-radius: 999px; }
  `,
})
export class Preferences {
  protected readonly theme = inject(ThemeService);
  protected readonly keyboard = inject(KeyboardService);
  protected readonly themes = THEMES;
  protected readonly keyboards = KEYBOARDS;
  protected setKeyboard(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    if (value === 'auto' || value === 'mac' || value === 'windows') {
      this.keyboard.setStyle(value);
    }
  }
}
