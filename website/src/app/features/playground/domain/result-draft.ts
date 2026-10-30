import { ResultStatusInfo } from '@shared/kernel/result-status';

export interface ProductSample {
  readonly id: number;
  readonly name: string;
  readonly price: number;
}

export interface ResultDraft {
  readonly status: ResultStatusInfo;
  readonly title: string;
  readonly detail: string;
  readonly argument: string;
  readonly errors: readonly string[];
  readonly fieldErrors: Readonly<Record<string, readonly string[]>>;
  readonly withData: boolean;
  readonly product: ProductSample;
  readonly passHttpContext: boolean;
  readonly requestPath: string;
}

export interface SimulatedResult {
  readonly isSuccessful: boolean;
  readonly status: ResultStatusInfo;
  readonly title: string;
  readonly detail: string | null;
  readonly errors: readonly string[];
  readonly fieldErrors: Readonly<Record<string, readonly string[]>>;
  readonly data: ProductSample | null;
  readonly isGeneric: boolean;
}

export type BuildMode = 'facade' | 'invalid' | 'fieldErrors' | 'errorList';
