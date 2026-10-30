import { ChangeDetectionStrategy, Component, computed, DestroyRef, DOCUMENT, effect, inject, signal, untracked } from '@angular/core';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { TabSyncService } from '@core/services/tab-sync.service';
import { CodeBlock } from '@shared/ui/code-block';
import { ApiStyle, HERO_FEATURES, HeroComposer, HeroFeature } from '../domain/hero-composer';

const STYLES: readonly ApiStyle[] = ['Minimal API', 'MVC controller'];
const CHARS_PER_TICK = 4;
const TICK_MS = 12;

@Component({
  selector: 'rh-hero-composer',
  imports: [CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './hero-composer.html',
  styleUrl: './hero-composer.scss',
})
export class HeroComposerView {
  private readonly composer = new HeroComposer();
  private readonly sync = inject(TabSyncService);
  private readonly document = inject(DOCUMENT);
  private readonly styleSelection = this.sync.selection(TAB_GROUPS.apiStyle);
  private timer: ReturnType<typeof setInterval> | null = null;

  protected readonly features = HERO_FEATURES;
  protected readonly styles = STYLES;
  protected readonly selected = signal<ReadonlySet<HeroFeature>>(new Set<HeroFeature>(['guard', 'map']));
  protected readonly typing = signal(true);
  protected readonly style = computed<ApiStyle>(() => (this.styleSelection() === 'MVC controller' ? 'MVC controller' : 'Minimal API'));
  protected readonly target = computed(() => this.composer.compose(this.selected(), this.style()));
  protected readonly outcomes = computed(() => this.composer.outcomes(this.selected()));
  protected readonly displayed = signal('');

  constructor() {
    inject(DestroyRef).onDestroy(() => this.stop());
    effect(() => {
      const target = this.target();
      const animate = this.typing() && !this.prefersReducedMotion();
      untracked(() => this.render(target, animate));
    });
  }

  protected toggle(feature: HeroFeature): void {
    this.selected.update(current => {
      const next = new Set(current);
      if (!next.delete(feature)) {
        next.add(feature);
      }
      return next;
    });
  }

  protected chooseStyle(style: ApiStyle): void {
    this.sync.select(TAB_GROUPS.apiStyle, style);
  }

  private render(target: string, animate: boolean): void {
    this.stop();
    const current = this.displayed();
    if (!animate || current === '') {
      this.displayed.set(target);
      return;
    }
    let position = commonPrefixLength(current, target);
    this.displayed.set(target.slice(0, position));
    this.timer = setInterval(() => {
      position = Math.min(target.length, position + CHARS_PER_TICK);
      this.displayed.set(target.slice(0, position));
      if (position >= target.length) {
        this.stop();
      }
    }, TICK_MS);
  }

  private stop(): void {
    if (this.timer !== null) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  private prefersReducedMotion(): boolean {
    return this.document.defaultView?.matchMedia('(prefers-reduced-motion: reduce)').matches ?? false;
  }
}

function commonPrefixLength(a: string, b: string): number {
  let index = 0;
  while (index < a.length && index < b.length && a[index] === b[index]) {
    index++;
  }
  return index;
}
