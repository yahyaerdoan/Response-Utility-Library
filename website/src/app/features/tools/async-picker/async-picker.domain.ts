export type SourceKind = 'value' | 'interfaceTask' | 'concreteTask';

export type StepKind = 'mapSync' | 'mapAsync' | 'bindData' | 'bindDataAsync' | 'bindDataConcrete' | 'bindNoData' | 'bindNoDataAsync' | 'bindNoDataConcrete';

export interface Choice<TKey extends string> {
  readonly key: TKey;
  readonly label: string;
  readonly type: string;
  readonly example: string;
}

export interface Resolution {
  readonly method: string;
  readonly returns: string;
  readonly since: string;
  readonly chain: string;
  readonly concrete: boolean;
}

export const SOURCES: readonly Choice<SourceKind>[] = [
  { key: 'value', label: 'A result you already have', type: 'IOperationResult<T>', example: 'var user = _users.Find(id);' },
  { key: 'interfaceTask', label: 'An async method returning the interface', type: 'Task<IOperationResult<T>>', example: '_users.FindAsync(id)' },
  { key: 'concreteTask', label: 'An async method returning the concrete type (e.g. MediatR Send)', type: 'Task<OperationDataResult<T>>', example: 'sender.Send(new GetUserQuery(id))' },
];

export const STEPS: readonly Choice<StepKind>[] = [
  { key: 'mapSync', label: 'Transform the data', type: 'Func<T, TOut>', example: 'user => user.Email' },
  { key: 'mapAsync', label: 'Transform the data asynchronously', type: 'Func<T, Task<TOut>>', example: 'user => _avatars.GetUrlAsync(user.Id)' },
  { key: 'bindData', label: 'Call something that returns a data result', type: 'Func<T, IOperationResult<TOut>>', example: 'user => _orders.Latest(user.Id)' },
  { key: 'bindDataAsync', label: 'Await something that returns a data result', type: 'Func<T, Task<IOperationResult<TOut>>>', example: 'user => _orders.LatestAsync(user.Id)' },
  { key: 'bindDataConcrete', label: 'Await a concrete data result (e.g. MediatR query)', type: 'Func<T, Task<OperationDataResult<TOut>>>', example: 'user => sender.Send(new GetOrdersQuery(user.Id))' },
  { key: 'bindNoData', label: 'Call something that returns no data', type: 'Func<T, IOperationResult>', example: 'user => _audit.Record(user.Id)' },
  { key: 'bindNoDataAsync', label: 'Await something that returns no data', type: 'Func<T, Task<IOperationResult>>', example: 'user => _mailer.SendWelcomeAsync(user.Email)' },
  { key: 'bindNoDataConcrete', label: 'Await a concrete no-data result (e.g. MediatR command)', type: 'Func<T, Task<OperationResult>>', example: 'user => sender.Send(new DeleteAddressCommand(id, user.Id))' },
];

const DATA = 'IOperationResult<TOut>';
const NO_DATA = 'IOperationResult';

export function resolve(source: SourceKind, step: StepKind): Resolution {
  const sourceChoice = SOURCES.find(choice => choice.key === source) ?? SOURCES[0];
  const stepChoice = STEPS.find(choice => choice.key === step) ?? STEPS[0];
  const isMap = step === 'mapSync' || step === 'mapAsync';
  const returnsData = isMap || step.startsWith('bindData');
  const syncOnly = source === 'value' && (step === 'mapSync' || step === 'bindData' || step === 'bindNoData');
  const method = (isMap ? 'Map' : 'Bind') + (syncOnly ? '' : 'Async');

  const concreteFlow = source === 'concreteTask' && (isMap || step === 'bindDataConcrete' || step === 'bindNoDataConcrete');
  const inner = concreteFlow
    ? (returnsData ? 'OperationDataResult<TOut>' : 'OperationResult')
    : (returnsData ? DATA : NO_DATA);
  const returns = syncOnly ? inner : `Task<${inner}>`;

  const prefix = source === 'value' ? 'user' : sourceChoice.example;
  const chain = `${syncOnly ? '' : 'await '}${prefix}\n    .${method}(${stepChoice.example})`;

  return { method, returns, since: sinceOf(source, step, returnsData), chain, concrete: concreteFlow };
}

function sinceOf(source: SourceKind, step: StepKind, returnsData: boolean): string {
  if (!returnsData) {
    return '13.1';
  }
  if (source === 'concreteTask' || step === 'bindDataConcrete') {
    return '13.0';
  }
  return '12.0';
}
