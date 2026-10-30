import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';
import { PageHeader } from '@shared/ui/page-header';
import { resolve, SourceKind, SOURCES, StepKind, STEPS } from './async-picker.domain';

@Component({
  selector: 'rh-async-picker-page',
  imports: [RouterLink, PageHeader, CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Tools" icon="bi-shuffle" title="Which overload will I get?"
                    lead="Pick what you start from and what the next step returns. See the method, the resulting type and the version it needs." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="row g-4">
        <div class="col-lg-6">
          <fieldset class="surface p-4 mb-4">
            <legend class="h6 fw-bold mb-3">1. You start from</legend>
            @for (choice of sources; track choice.key) {
              <label class="option" [class.active]="source() === choice.key">
                <input type="radio" name="source" class="form-check-input" [checked]="source() === choice.key" (change)="source.set(choice.key)" />
                <span class="min-w-0">
                  <span class="d-block fw-semibold">{{ choice.label }}</span>
                  <code class="small">{{ choice.type }}</code>
                </span>
              </label>
            }
          </fieldset>

          <fieldset class="surface p-4">
            <legend class="h6 fw-bold mb-3">2. The next step</legend>
            @for (choice of steps; track choice.key) {
              <label class="option" [class.active]="step() === choice.key">
                <input type="radio" name="step" class="form-check-input" [checked]="step() === choice.key" (change)="step.set(choice.key)" />
                <span class="min-w-0">
                  <span class="d-block fw-semibold">{{ choice.label }}</span>
                  <code class="small">{{ choice.type }}</code>
                </span>
              </label>
            }
          </fieldset>
        </div>

        <div class="col-lg-6">
          @let result = resolution();
          <section class="surface p-4 sticky-lg-top result" aria-live="polite">
            <span class="eyebrow">Result</span>
            <dl class="row mt-2 mb-3 small">
              <dt class="col-4">Method</dt><dd class="col-8"><code>{{ result.method }}</code></dd>
              <dt class="col-4">Returns</dt><dd class="col-8"><code>{{ result.returns }}</code></dd>
              <dt class="col-4">Available since</dt><dd class="col-8">{{ result.since }}</dd>
            </dl>
            <rh-code-block [code]="result.chain" language="csharp" />
            @if (result.concrete) {
              <p class="small mt-3 mb-0"><i class="bi bi-check-circle me-1 text-success"></i>The concrete type flows through, so a handler can return this chain directly.</p>
            } @else {
              <p class="small text-muted-rh mt-3 mb-0"><i class="bi bi-info-circle me-1"></i>Once an interface enters the chain, the result stays an interface. Declare the concrete <code>OperationDataResult&lt;T&gt;</code> on both sides to keep it.</p>
            }
            <p class="small text-muted-rh mt-3 mb-0">Background: <a routerLink="/docs/async" fragment="services">async composition</a>.</p>
          </section>
        </div>
      </div>
    </div>
  `,
  styles: `
    .option { display: flex; align-items: flex-start; gap: .75rem; padding: .6rem .75rem; border: 1px solid transparent; border-radius: .6rem; cursor: pointer; }
    .option + .option { margin-top: .25rem; }
    .option:hover { background: var(--rh-surface-2); }
    .option.active { border-color: var(--rh-accent); background: var(--rh-accent-soft); }
    .option .form-check-input { margin-top: .3rem; flex-shrink: 0; }
    .result { top: 6rem; }
    dt { color: var(--rh-muted); font-weight: 600; }
  `,
})
export class AsyncPickerPage {
  protected readonly sources = SOURCES;
  protected readonly steps = STEPS;
  protected readonly source = signal<SourceKind>('concreteTask');
  protected readonly step = signal<StepKind>('bindNoDataConcrete');
  protected readonly resolution = computed(() => resolve(this.source(), this.step()));
}
