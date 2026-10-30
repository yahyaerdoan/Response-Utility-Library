import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CalloutTone } from '@core/domain/content.model';

interface CalloutIcon {
  readonly idle: string;
  readonly active?: string;
}

const ICONS: Record<CalloutTone, CalloutIcon> = {
  info: { idle: 'bi-info-circle' },
  tip: { idle: 'bi-lightbulb', active: 'bi-lightbulb-fill' },
  warning: { idle: 'bi-exclamation-triangle' },
  success: { idle: 'bi-check-circle' },
};

@Component({
  selector: 'rh-callout',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="callout" [class]="'callout tone-' + tone()">
      <span class="icon" aria-hidden="true">
        <i class="bi idle" [class]="icon().idle"></i>
        @if (icon().active; as active) {
          <i class="bi active" [class]="active"></i>
        }
      </span>
      <div class="flex-grow-1 min-w-0">
        @if (title()) {
          <strong class="d-block mb-1">{{ title() }}</strong>
        }
        <ng-content />
      </div>
    </aside>
  `,
  styles: `
    .callout { display: flex; gap: .8rem; padding: .9rem 1.1rem; border-radius: var(--bs-border-radius-lg); border: 1px solid; font-size: .95rem; transition: border-color .25s ease, background-color .25s ease; }
    .tone-info { --c: var(--rh-info); }
    .tone-tip { --c: var(--rh-accent); --lit: #ffc83d; }
    .tone-warning { --c: var(--rh-warning); }
    .tone-success { --c: var(--rh-success); }
    .callout { border-color: color-mix(in srgb, var(--c) 35%, transparent); background: color-mix(in srgb, var(--c) 8%, transparent); }
    strong { color: var(--c); }

    .icon { position: relative; display: inline-grid; flex-shrink: 0; font-size: 1.1rem; line-height: 1.4; color: var(--c); }
    .icon .bi { grid-area: 1 / 1; transition: opacity .25s ease, color .25s ease, filter .25s ease; }
    .icon .active { opacity: 0; color: var(--lit); }

    .tone-tip:hover,
    .tone-tip:focus-within { border-color: color-mix(in srgb, var(--lit) 45%, transparent); background: color-mix(in srgb, var(--lit) 7%, transparent); }
    .tone-tip:hover .idle,
    .tone-tip:focus-within .idle { opacity: 0; }
    .tone-tip:hover .active,
    .tone-tip:focus-within .active { opacity: 1; filter: drop-shadow(0 0 .35rem color-mix(in srgb, var(--lit) 85%, transparent)); animation: switch-on .45s ease-out; }

    @keyframes switch-on {
      0% { opacity: 0; }
      30% { opacity: 1; }
      45% { opacity: .35; }
      60%, 100% { opacity: 1; }
    }
  `,
})
export class Callout {
  readonly tone = input<CalloutTone>('info');
  readonly title = input<string>();
  protected readonly icon = computed(() => ICONS[this.tone()]);
}
