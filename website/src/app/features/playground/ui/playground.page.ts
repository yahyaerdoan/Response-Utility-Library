import { afterNextRender, ChangeDetectionStrategy, Component, computed, DestroyRef, DOCUMENT, effect, ElementRef, inject, input, linkedSignal, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { findStatus, RESULT_STATUSES, ResultStatusInfo, STATUS_CLASS_LABELS, StatusClass } from '@shared/kernel/result-status';
import { Callout } from '@shared/ui/callout';
import { CodeBlock } from '@shared/ui/code-block';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { PageHeader } from '@shared/ui/page-header';
import { AssertionGenerator } from '../domain/assertion-generator';
import { CSharpGenerator } from '../domain/csharp-generator';
import { PLAYGROUND_PRESETS, PlaygroundPreset } from '../domain/presets';
import { RESPONSE_ADAPTERS, ResponseAdapter } from '../domain/response-adapters';
import { ResultBuilder } from '../domain/result-builder';
import { ResultDraft } from '../domain/result-draft';

type OutputTab = 'code' | 'response' | 'test';

@Component({
  selector: 'rh-playground-page',
  imports: [FormsModule, PageHeader, CodeBlock, Callout, InlineMarkupPipe],
  providers: [ResultBuilder, AssertionGenerator, { provide: CSharpGenerator, useFactory: () => new CSharpGenerator(inject(ResultBuilder)) }],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './playground.page.html',
  styleUrl: './playground.page.scss',
})
export class PlaygroundPage {
  private readonly builder = inject(ResultBuilder);
  private readonly csharp = inject(CSharpGenerator);
  private readonly assertions = inject(AssertionGenerator);

  private readonly document = inject(DOCUMENT);
  private readonly output = viewChild.required<ElementRef<HTMLElement>>('output');

  readonly status = input<string>();

  protected readonly outputVisible = signal(true);

  protected readonly presets = PLAYGROUND_PRESETS;
  protected readonly adapters = RESPONSE_ADAPTERS;
  protected readonly groups = ([1, 2, 3, 4, 5] as StatusClass[]).map(statusClass => ({
    label: `${statusClass}xx · ${STATUS_CLASS_LABELS[statusClass]}`,
    statuses: RESULT_STATUSES.filter(status => status.statusClass === statusClass),
  }));

  protected readonly statusName = linkedSignal(() => findStatus(this.status() ?? '')?.name ?? 'NotFound');
  protected readonly title = signal('');
  protected readonly detail = signal('Product 42 does not exist.');
  protected readonly argument = signal('/api/v2/products/42');
  protected readonly errorsText = signal('');
  protected readonly fieldErrorsText = signal('');
  protected readonly withData = signal(true);
  protected readonly productName = signal('Duct cleaning kit');
  protected readonly productPrice = signal(129.9);
  protected readonly passHttpContext = signal(true);
  protected readonly activePreset = signal<string | null>(null);
  protected readonly outputTab = signal<OutputTab>('response');
  protected readonly adapterId = signal(RESPONSE_ADAPTERS[0].id);

  protected readonly selectedStatus = computed<ResultStatusInfo>(() => findStatus(this.statusName()) ?? RESULT_STATUSES[0]);
  protected readonly adapter = computed<ResponseAdapter>(() => this.adapters.find(a => a.id === this.adapterId()) ?? this.adapters[0]);

  protected readonly draft = computed<ResultDraft>(() => ({
    status: this.selectedStatus(),
    title: this.title(),
    detail: this.detail(),
    argument: this.argument(),
    errors: parseLines(this.errorsText()),
    fieldErrors: parseFieldErrors(this.fieldErrorsText()),
    withData: this.withData(),
    product: { id: 42, name: this.productName(), price: this.productPrice() },
    passHttpContext: this.passHttpContext(),
    requestPath: '/api/products/42',
  }));

  protected readonly result = computed(() => this.builder.build(this.draft()));
  protected readonly typeLabel = computed(() => (this.result().isGeneric ? 'IOperationResult<ProductDto>' : 'IOperationResult'));
  protected readonly mode = computed(() => this.builder.mode(this.draft()));
  protected readonly code = computed(() => this.csharp.generate(this.draft()));
  protected readonly test = computed(() => this.assertions.generate(this.result()));
  protected readonly response = computed(() => this.adapter().render(this.result(), this.draft()));

  protected readonly modeHint = computed(() => {
    switch (this.mode()) {
      case 'fieldErrors':
        return 'Field errors present → `OperationResult.Failure(fieldErrors)` wins: status becomes **422 Validation Failed** and `Errors` is the flattened list.';
      case 'invalid':
        return 'Error list + 422 → `Result.Invalid(...)`, the idiomatic validation factory.';
      case 'errorList':
        return 'Error list with a non-422 status → the `ErrorResult(title, status, errors)` constructor.';
      default:
        return null;
    }
  });

  constructor() {
    const destroyRef = inject(DestroyRef);
    effect(() => this.document.documentElement.style.setProperty('--rh-fixed-bottom', this.outputVisible() ? '0px' : '4.5rem'));
    afterNextRender(() => {
      if (typeof IntersectionObserver === 'undefined') {
        return;
      }
      const observer = new IntersectionObserver(([entry]) => this.outputVisible.set(entry.isIntersecting), { rootMargin: '0px 0px -40% 0px' });
      observer.observe(this.output().nativeElement);
      destroyRef.onDestroy(() => {
        observer.disconnect();
        this.document.documentElement.style.removeProperty('--rh-fixed-bottom');
      });
    });
  }

  protected scrollToOutput(): void {
    this.output().nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  protected applyPreset(preset: PlaygroundPreset): void {
    this.activePreset.set(preset.id);
    this.statusName.set(preset.status);
    this.withData.set(preset.withData ?? false);
    this.detail.set(preset.detail ?? '');
    this.argument.set(preset.argument ?? '/api/v2/products/42');
    this.errorsText.set(preset.errors ?? '');
    this.fieldErrorsText.set(preset.fieldErrors ?? '');
    this.title.set('');
  }

  protected selectStatus(name: string): void {
    this.activePreset.set(null);
    this.statusName.set(name);
  }
}

function parseLines(text: string): string[] {
  return text.split('\n').map(line => line.trim()).filter(Boolean);
}

function parseFieldErrors(text: string): Record<string, string[]> {
  const map: Record<string, string[]> = {};
  for (const line of parseLines(text)) {
    const separator = line.indexOf(':');
    if (separator <= 0) {
      continue;
    }
    const field = line.slice(0, separator).trim();
    const message = line.slice(separator + 1).trim();
    if (field && message) {
      (map[field] ??= []).push(message);
    }
  }
  return map;
}
