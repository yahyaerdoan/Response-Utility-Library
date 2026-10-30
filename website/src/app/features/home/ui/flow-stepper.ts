import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, input, signal, untracked } from '@angular/core';
import { CodeBlock } from '@shared/ui/code-block';
import { inView, prefersReducedMotion } from '@shared/util/in-view';
import { FlowStep } from '../home.content';

const STEP_MS = 3500;

@Component({
  selector: 'rh-flow-stepper',
  imports: [CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stepper">
      <div class="head">
        <ol class="steps list-unstyled mb-0" role="tablist" aria-label="From method to response, explicitly">
          @for (item of steps(); track item.label; let index = $index) {
            <li role="presentation">
              <button type="button" role="tab" class="step" [class.active]="index === active()" [class.done]="index < active()"
                      [attr.aria-selected]="index === active()" (click)="select(index)">
                <span class="icon-tile"><i class="bi" [class]="item.icon"></i></span>
                <span class="text"><strong>{{ item.label }}</strong><small class="font-monospace">{{ item.caption }}</small></span>
                @if (index === active() && !paused()) {
                  <span class="progress-line" [style.animation-duration.ms]="stepMs"></span>
                }
              </button>
            </li>
          }
        </ol>
        <button type="button" class="pause" (click)="paused.set(!paused())" [attr.aria-label]="paused() ? 'Play flow animation' : 'Pause flow animation'">
          <i class="bi" [class]="paused() ? 'bi-play-fill' : 'bi-pause-fill'"></i>
        </button>
      </div>
      @if (current(); as item) {
        <rh-code-block class="detail" [code]="item.snippet.code" [language]="item.snippet.language" [title]="item.snippet.label" />
      }
    </div>
  `,
  styles: `
    :host { display: block; }
    .head { display: flex; gap: .75rem; align-items: stretch; }
    .steps { flex: 1; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: .75rem; }
    .step { position: relative; overflow: hidden; display: flex; align-items: center; gap: .75rem; width: 100%; height: 100%; padding: .9rem 1rem; text-align: left; border: 1px solid var(--rh-border); border-radius: .9rem; background: var(--rh-surface); color: var(--rh-text); transition: border-color .2s ease, background-color .2s ease; }
    .step:hover { border-color: color-mix(in srgb, var(--rh-accent) 50%, var(--rh-border)); }
    .step.active { border-color: var(--rh-accent); background: var(--rh-accent-soft); }
    .step.done .icon-tile { background: color-mix(in srgb, var(--rh-success) 15%, transparent); color: var(--rh-success); }
    .text { display: flex; flex-direction: column; min-width: 0; }
    .text small { color: var(--rh-muted); font-size: .72rem; overflow-wrap: anywhere; }
    .progress-line { position: absolute; left: 0; bottom: 0; height: 3px; width: 100%; background: var(--rh-accent); transform-origin: left; animation: fill linear forwards; }
    @keyframes fill { from { transform: scaleX(0); } to { transform: scaleX(1); } }
    .pause { flex: none; width: 2.5rem; border: 1px solid var(--rh-border); border-radius: .9rem; background: var(--rh-surface); color: var(--rh-text); }
    .pause:hover { border-color: var(--rh-accent); }
    .detail { display: block; margin-top: 1rem; animation: appear .25s ease-out; }
    @keyframes appear { from { opacity: 0; transform: translateY(6px); } }
    @media (max-width: 991.98px) { .steps { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
    @media (max-width: 575.98px) { .step { padding: .7rem; gap: .5rem; } .step .icon-tile { width: 2rem; height: 2rem; font-size: 1rem; } }
  `,
})
export class FlowStepper {
  private readonly visible = inView(0.4);
  private timer: ReturnType<typeof setInterval> | null = null;

  readonly steps = input.required<readonly FlowStep[]>();

  protected readonly stepMs = STEP_MS;
  protected readonly active = signal(0);
  protected readonly paused = signal(prefersReducedMotion());
  protected readonly current = computed(() => this.steps()[this.active()]);
  private readonly running = computed(() => this.visible() && !this.paused());

  constructor() {
    inject(DestroyRef).onDestroy(() => this.stop());
    effect(() => {
      const running = this.running();
      untracked(() => (running ? this.start() : this.stop()));
    });
  }

  protected select(index: number): void {
    this.active.set(index);
    this.paused.set(true);
  }

  private start(): void {
    this.stop();
    this.timer = setInterval(() => this.active.update(index => (index + 1) % this.steps().length), STEP_MS);
  }

  private stop(): void {
    if (this.timer !== null) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }
}
