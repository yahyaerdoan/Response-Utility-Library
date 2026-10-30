import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { inView, prefersReducedMotion } from '@shared/util/in-view';
import { TERMINAL_SCRIPT, TerminalLine } from '../domain/terminal-script';

const TYPE_DELAY = 22;
const LINE_DELAY = 450;
const LOOP_DELAY = 4000;

@Component({
  selector: 'rh-terminal-story',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="terminal" aria-label="Terminal demo" role="img" [attr.aria-description]="transcript">
      <div class="bar">
        <span class="dots" aria-hidden="true"><i></i><i></i><i></i></span>
        <span class="name">bash · shop-api</span>
        <span class="controls">
          <button type="button" (click)="restart()" aria-label="Replay"><i class="bi bi-arrow-counterclockwise"></i></button>
          <button type="button" (click)="togglePlay()" [attr.aria-label]="paused() ? 'Play terminal story' : 'Pause terminal story'">
            <i class="bi" [class]="paused() ? 'bi-play-fill' : 'bi-pause-fill'"></i>
          </button>
        </span>
      </div>
      <div class="screen" aria-hidden="true">
        @for (line of shown(); track $index) {
          <div [class]="'line ' + line.kind">@if (line.kind === 'command') {<span class="prompt">$ </span>}{{ line.text }}</div>
        }
        @if (typing(); as partial) {
          <div class="line command"><span class="prompt">$ </span>{{ partial }}<span class="caret"></span></div>
        }
      </div>
    </div>
  `,
  styles: `
    :host { display: block; }
    .terminal { border-radius: 1rem; overflow: hidden; border: 1px solid #2a2548; background: #0b0918; color: #e3e1f3; box-shadow: 0 2rem 4rem -2rem rgba(0, 0, 0, .6); }
    .bar { display: flex; align-items: center; gap: .75rem; padding: .55rem .85rem; background: #15122a; border-bottom: 1px solid #2a2548; }
    .dots { display: inline-flex; gap: .35rem; }
    .dots i { width: .65rem; height: .65rem; border-radius: 50%; background: #3a3460; }
    .dots i:nth-child(1) { background: #ff5f57; } .dots i:nth-child(2) { background: #febc2e; } .dots i:nth-child(3) { background: #28c840; }
    .name { flex: 1; font-family: var(--bs-font-monospace); font-size: .75rem; color: #a3a0bd; }
    .controls { display: inline-flex; gap: .25rem; }
    .controls button { border: 1px solid #2a2548; background: transparent; color: #cfcbe8; border-radius: .4rem; padding: .15rem .45rem; line-height: 1; }
    .controls button:hover { border-color: #9b83ff; color: #fff; }
    .screen { padding: 1rem 1.1rem; height: 23rem; overflow: hidden; font-family: var(--bs-font-monospace); font-variant-ligatures: none; font-size: .8rem; line-height: 1.55; display: flex; flex-direction: column; justify-content: flex-end; }
    .line { white-space: pre-wrap; overflow-wrap: anywhere; }
    .prompt { color: #4ad295; }
    .command { color: #fff; }
    .output { color: #a3a0bd; }
    .comment { color: #6f6a8e; font-style: italic; margin-top: .4rem; }
    .json { color: #a5e075; }
    .caret { display: inline-block; width: .5rem; height: 1rem; margin-left: 1px; vertical-align: text-bottom; background: #9b83ff; animation: blink 1s steps(1) infinite; }
    @keyframes blink { 50% { opacity: 0; } }
    @media (max-width: 575.98px) { .screen { height: 20rem; font-size: .7rem; } }
  `,
})
export class TerminalStory {
  private readonly visible = inView();
  private timer: ReturnType<typeof setTimeout> | null = null;
  private lineIndex = 0;
  private charIndex = 0;

  protected readonly transcript = TERMINAL_SCRIPT.map(line => line.text).join('\n');
  protected readonly paused = signal(false);
  protected readonly shown = signal<readonly TerminalLine[]>([]);
  protected readonly typing = signal<string | null>(null);
  private readonly running = computed(() => this.visible() && !this.paused());

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clear());
    if (prefersReducedMotion()) {
      this.shown.set(TERMINAL_SCRIPT);
      this.lineIndex = TERMINAL_SCRIPT.length;
      this.paused.set(true);
    }
    effect(() => {
      const running = this.running();
      untracked(() => (running ? this.schedule(0) : this.clear()));
    });
  }

  protected togglePlay(): void {
    if (this.paused() && this.lineIndex >= TERMINAL_SCRIPT.length) {
      this.restart();
      return;
    }
    this.paused.update(paused => !paused);
  }

  protected restart(): void {
    this.clear();
    this.lineIndex = 0;
    this.charIndex = 0;
    this.shown.set([]);
    this.typing.set(null);
    this.paused.set(false);
    if (this.running()) {
      this.schedule(0);
    }
  }

  private schedule(delay: number): void {
    this.clear();
    this.timer = setTimeout(() => this.tick(), delay);
  }

  private tick(): void {
    if (this.lineIndex >= TERMINAL_SCRIPT.length) {
      this.timer = setTimeout(() => this.restart(), LOOP_DELAY);
      return;
    }
    const line = TERMINAL_SCRIPT[this.lineIndex];
    if (line.kind === 'command' && this.charIndex < line.text.length) {
      this.charIndex += 2;
      this.typing.set(line.text.slice(0, this.charIndex));
      this.schedule(TYPE_DELAY);
      return;
    }
    this.typing.set(null);
    this.shown.update(lines => [...lines, line]);
    this.lineIndex++;
    this.charIndex = 0;
    this.schedule(line.kind === 'command' ? 250 : LINE_DELAY);
  }

  private clear(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
