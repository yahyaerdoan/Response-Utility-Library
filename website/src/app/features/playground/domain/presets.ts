export interface PlaygroundPreset {
  readonly id: string;
  readonly label: string;
  readonly icon: string;
  readonly status: string;
  readonly withData?: boolean;
  readonly detail?: string;
  readonly argument?: string;
  readonly errors?: string;
  readonly fieldErrors?: string;
}

export const PLAYGROUND_PRESETS: readonly PlaygroundPreset[] = [
  { id: 'found', label: 'Product found', icon: 'bi-check2-circle', status: 'Ok', withData: true },
  { id: 'created', label: 'Created', icon: 'bi-plus-circle', status: 'Created', withData: true },
  { id: 'deleted', label: 'Deleted', icon: 'bi-trash3', status: 'NoContent' },
  { id: 'missing', label: 'Not found', icon: 'bi-question-circle', status: 'NotFound', withData: true, detail: 'Product 42 does not exist.' },
  { id: 'invalid', label: 'Validation list', icon: 'bi-list-check', status: 'UnprocessableContent', errors: 'Name is required.\nPrice must be greater than zero.' },
  { id: 'fields', label: 'Field errors', icon: 'bi-ui-checks', status: 'UnprocessableContent', fieldErrors: 'Name: Name is required.\nPrice: Price must be greater than zero.\nPrice: Price must have two decimals.' },
  { id: 'duplicate', label: 'Conflict', icon: 'bi-files', status: 'Conflict', detail: "A product named 'Duct cleaning kit' already exists." },
  { id: 'forbidden', label: 'Forbidden', icon: 'bi-shield-lock', status: 'Forbidden' },
  { id: 'moved', label: 'Moved', icon: 'bi-signpost-split', status: 'MovedPermanently', argument: '/api/v2/products/42' },
  { id: 'throttled', label: 'Rate limited', icon: 'bi-speedometer2', status: 'TooManyRequests' },
];
