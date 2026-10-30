export interface PipelineState {
  readonly ok: boolean;
  readonly type: string;
  readonly value: string;
  readonly status: string;
  readonly code: number;
  readonly title: string;
  readonly detail: string | null;
}

export type StepOutcome = 'ran' | 'skipped' | 'failed';

export interface StepTrace {
  readonly step: PipelineStep;
  readonly outcome: StepOutcome;
  readonly state: PipelineState;
  readonly log: string | null;
}

export abstract class PipelineStep {
  abstract readonly id: string;
  abstract readonly method: string;
  abstract readonly explanation: string;
  readonly failureToggle: string | null = null;
  readonly standalone: boolean = false;

  abstract code(async: boolean): string;
  abstract apply(state: PipelineState, shouldFail: boolean): Omit<StepTrace, 'step'>;

  protected skip(state: PipelineState): Omit<StepTrace, 'step'> {
    return { outcome: 'skipped', state, log: null };
  }
}

const suffix = (async: boolean): string => (async ? 'Async' : '');

export class SourceStep extends PipelineStep {
  readonly id = 'source';
  readonly method = 'GetById';
  readonly explanation = 'The repository call that starts the chain. Everything after it only runs while the result is successful.';
  override readonly failureToggle = 'Product does not exist';

  code(async: boolean): string {
    return async ? '_products.GetByIdAsync(id)' : '_products.GetById(id)';
  }

  apply(_: PipelineState, shouldFail: boolean): Omit<StepTrace, 'step'> {
    if (shouldFail) {
      return {
        outcome: 'failed',
        state: { ok: false, type: 'IOperationResult<Product>', value: '', status: 'NotFound', code: 404, title: 'Not Found', detail: 'Product 42 does not exist.' },
        log: null,
      };
    }
    return {
      outcome: 'ran',
      state: { ok: true, type: 'IOperationResult<Product>', value: 'Product { Id = 42, Name = "Duct cleaning kit", Stock = 3 }', status: 'Ok', code: 200, title: 'Operation completed successfully.', detail: null },
      log: null,
    };
  }
}

export class EnsureStep extends PipelineStep {
  readonly id = 'ensure';
  readonly method = 'Ensure';
  readonly explanation = 'A guard clause. Turns a still-successful result into a 422 failure when the business rule rejects the data.';
  override readonly failureToggle = 'Out of stock (Stock = 0)';

  code(async: boolean): string {
    return `.Ensure${suffix(async)}(p => p.Stock > 0, "Product is out of stock.")`;
  }

  apply(state: PipelineState, shouldFail: boolean): Omit<StepTrace, 'step'> {
    if (!state.ok) {
      return this.skip(state);
    }
    if (shouldFail) {
      return {
        outcome: 'failed',
        state: { ...state, ok: false, value: '', status: 'UnprocessableContent', code: 422, title: 'Validation Failed', detail: null },
        log: 'errors: ["Product is out of stock."]',
      };
    }
    return { outcome: 'ran', state, log: 'predicate returned true' };
  }
}

export class MapStep extends PipelineStep {
  readonly id = 'map';
  readonly method = 'Map';
  readonly explanation = 'Transforms the data (Product → ProductDto). Never fails by itself; on a failed input the mapper is not even called.';

  code(async: boolean): string {
    return `.Map${suffix(async)}(p => ToDto(p))`;
  }

  apply(state: PipelineState): Omit<StepTrace, 'step'> {
    if (!state.ok) {
      return { outcome: 'skipped', state: { ...state, type: 'IOperationResult<ProductDto>' }, log: 'failure re-projected into the new type' };
    }
    return {
      outcome: 'ran',
      state: { ...state, type: 'IOperationResult<ProductDto>', value: 'ProductDto { Id = 42, Name = "Duct cleaning kit" }' },
      log: null,
    };
  }
}

export class BindStep extends PipelineStep {
  readonly id = 'bind';
  readonly method = 'Bind';
  readonly explanation = 'Chains into another result-returning call. Its own failure (here: 409 Conflict) becomes the chain’s failure.';
  override readonly failureToggle = 'Draft order already exists';

  code(async: boolean): string {
    return async ? '.BindAsync(dto => _orders.CreateDraftOrderAsync(dto))' : '.Bind(dto => _orders.CreateDraftOrder(dto))';
  }

  apply(state: PipelineState, shouldFail: boolean): Omit<StepTrace, 'step'> {
    const type = 'IOperationResult<OrderDto>';
    if (!state.ok) {
      return { outcome: 'skipped', state: { ...state, type }, log: 'failure re-projected into the new type' };
    }
    if (shouldFail) {
      return {
        outcome: 'failed',
        state: { ...state, ok: false, type, value: '', status: 'Conflict', code: 409, title: 'Conflict', detail: 'A draft order for product 42 already exists.' },
        log: null,
      };
    }
    return { outcome: 'ran', state: { ...state, type, value: 'OrderDto { Id = 1001, ProductId = 42 }', status: 'Created', code: 201, title: 'Resource created successfully.' }, log: null };
  }
}

export class OnSuccessStep extends PipelineStep {
  readonly id = 'onSuccess';
  readonly method = 'OnSuccess';
  readonly explanation = 'Side effect on success only (logging, events, e-mails). Returns the same result unchanged.';

  code(async: boolean): string {
    return async ? '.OnSuccessAsync(order => _email.SendConfirmationAsync(order))' : '.OnSuccess(order => _logger.LogInformation("Order {Id} drafted", order.Id))';
  }

  apply(state: PipelineState): Omit<StepTrace, 'step'> {
    return state.ok ? { outcome: 'ran', state, log: 'info: Order 1001 drafted' } : this.skip(state);
  }
}

export class OnFailureStep extends PipelineStep {
  readonly id = 'onFailure';
  readonly method = 'OnFailure';
  readonly explanation = 'Side effect on failure only. It is defined on the non-generic IOperationResult and returns it, so call it as its own statement to keep the typed result.';
  override readonly standalone = true;

  code(async: boolean): string {
    return async
      ? 'await order.OnFailureAsync(f => _alerts.NotifyAsync(f.Title));'
      : 'order.OnFailure(f => _logger.LogWarning("Failed: {Title}", f.Title));';
  }

  apply(state: PipelineState): Omit<StepTrace, 'step'> {
    return state.ok ? this.skip(state) : { outcome: 'ran', state, log: `warn: Failed: ${state.title}` };
  }
}

export const DEFAULT_PIPELINE: readonly PipelineStep[] = [
  new SourceStep(),
  new EnsureStep(),
  new MapStep(),
  new BindStep(),
  new OnSuccessStep(),
  new OnFailureStep(),
];

const INITIAL: PipelineState = { ok: true, type: '', value: '', status: 'Ok', code: 200, title: '', detail: null };

export function runPipeline(steps: readonly PipelineStep[], failures: ReadonlySet<string>): readonly StepTrace[] {
  const traces: StepTrace[] = [];
  let state = INITIAL;
  for (const step of steps) {
    const trace = step.apply(state, failures.has(step.id));
    traces.push({ step, ...trace });
    state = trace.state;
  }
  return traces;
}
