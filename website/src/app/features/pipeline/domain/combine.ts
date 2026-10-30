export interface IndependentCheck {
  readonly id: string;
  readonly label: string;
  readonly field: string;
  readonly message: string;
}

export const CHECKS: readonly IndependentCheck[] = [
  { id: 'name', label: 'Name is empty', field: 'Name', message: 'Name is required.' },
  { id: 'price', label: 'Price is 0', field: 'Price', message: 'Price must be greater than zero.' },
  { id: 'sku', label: 'SKU is taken', field: 'Sku', message: "SKU 'DC-42' is already in use." },
];

export interface AggregateOutcome {
  readonly ok: boolean;
  readonly errors: readonly string[];
}

export function firstFailure(failing: readonly IndependentCheck[]): AggregateOutcome {
  const first = failing[0];
  return first ? { ok: false, errors: [first.message] } : { ok: true, errors: [] };
}

export function combineAll(failing: readonly IndependentCheck[]): AggregateOutcome {
  return failing.length > 0 ? { ok: false, errors: failing.map(check => check.message) } : { ok: true, errors: [] };
}
