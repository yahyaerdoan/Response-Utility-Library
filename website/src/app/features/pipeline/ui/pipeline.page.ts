import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { Callout } from '@shared/ui/callout';
import { CodeBlock } from '@shared/ui/code-block';
import { PageHeader } from '@shared/ui/page-header';
import { CHECKS, combineAll, firstFailure, IndependentCheck } from '../domain/combine';
import { DEFAULT_PIPELINE, runPipeline } from '../domain/pipeline';

@Component({
  selector: 'rh-pipeline-page',
  imports: [PageHeader, CodeBlock, Callout],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './pipeline.page.html',
  styleUrl: './pipeline.page.scss',
})
export class PipelinePage {
  protected readonly steps = DEFAULT_PIPELINE;
  protected readonly checks = CHECKS;

  protected readonly async = signal(false);
  protected readonly failures = signal<ReadonlySet<string>>(new Set());
  protected readonly failingChecks = signal<ReadonlySet<string>>(new Set(['name', 'price']));

  protected readonly traces = computed(() => runPipeline(this.steps, this.failures()));
  protected readonly final = computed(() => this.traces()[this.traces().length - 1].state);
  protected readonly matchOutput = computed(() => {
    const state = this.final();
    return state.ok ? `"Drafted: ${state.value}"` : `"Error: ${state.title}"`;
  });

  protected readonly chainCode = computed(() => {
    const async = this.async();
    const head = async ? 'IOperationResult<OrderDto> order = await ' : 'IOperationResult<OrderDto> order = ';
    const chained = this.steps.filter(step => !step.standalone);
    const statements = this.steps.filter(step => step.standalone).map(step => step.code(async));
    const lines = chained.map((step, index) => (index === 0 ? head + step.code(async) : `    ${step.code(async)}`));
    return [`${lines.join('\n')};`, ...statements, `return order.ToActionResult(); // ${this.final().code}`].join('\n\n');
  });

  protected readonly selectedChecks = computed<readonly IndependentCheck[]>(() => this.checks.filter(check => this.failingChecks().has(check.id)));
  protected readonly ensureOutcome = computed(() => firstFailure(this.selectedChecks()));
  protected readonly combineOutcome = computed(() => combineAll(this.selectedChecks()));

  protected readonly combineCode = `var nameCheck  = string.IsNullOrEmpty(request.Name) ? Result.Invalid("Name is required.") : Result.Success();
var priceCheck = request.Price <= 0 ? Result.Invalid("Price must be greater than zero.") : Result.Success();
var skuCheck   = await _skus.ExistsAsync(request.Sku) ? Result.Conflict($"SKU '{request.Sku}' is already in use.") : Result.Success();

IOperationResult combined = Result.Combine(nameCheck, priceCheck, skuCheck);`;

  protected statusClass(code: number): number {
    return Math.floor(code / 100);
  }

  protected toggleFailure(id: string): void {
    this.failures.update(set => toggle(set, id));
  }

  protected toggleCheck(id: string): void {
    this.failingChecks.update(set => toggle(set, id));
  }

  protected reset(): void {
    this.failures.set(new Set());
  }
}

function toggle(set: ReadonlySet<string>, id: string): ReadonlySet<string> {
  const next = new Set(set);
  if (!next.delete(id)) {
    next.add(id);
  }
  return next;
}
