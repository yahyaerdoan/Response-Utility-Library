import { findStatus, ResultStatusInfo } from '@shared/kernel/result-status';
import { BuildMode, ResultDraft, SimulatedResult } from './result-draft';

const VALIDATION_FAILED = 'Validation Failed';

export class ResultBuilder {
  mode(draft: ResultDraft): BuildMode {
    if (draft.status.isSuccessful) {
      return 'facade';
    }
    if (Object.keys(draft.fieldErrors).length > 0) {
      return 'fieldErrors';
    }
    if (draft.errors.length > 0) {
      return draft.status.name === 'UnprocessableContent' ? 'invalid' : 'errorList';
    }
    return 'facade';
  }

  build(draft: ResultDraft): SimulatedResult {
    const isGeneric = this.isGeneric(draft);
    switch (this.mode(draft)) {
      case 'fieldErrors':
        return this.failure(draft, this.unprocessable(draft.status), VALIDATION_FAILED, Object.values(draft.fieldErrors).flat(), draft.fieldErrors, isGeneric);
      case 'invalid':
        return this.failure(draft, draft.status, VALIDATION_FAILED, draft.errors, {}, isGeneric);
      case 'errorList':
        return this.failure(draft, draft.status, draft.status.defaultTitle, draft.errors, {}, isGeneric);
      case 'facade':
        return draft.status.isSuccessful ? this.success(draft, isGeneric) : this.facadeFailure(draft, isGeneric);
    }
  }

  isGeneric(draft: ResultDraft): boolean {
    return draft.withData && draft.status.hasGenericFactory;
  }

  private success(draft: ResultDraft, isGeneric: boolean): SimulatedResult {
    return {
      isSuccessful: true,
      status: draft.status,
      title: this.successTitle(draft),
      detail: null,
      errors: [],
      fieldErrors: {},
      data: isGeneric ? draft.product : null,
      isGeneric,
    };
  }

  private facadeFailure(draft: ResultDraft, isGeneric: boolean): SimulatedResult {
    const detail = draft.detail.trim() || draft.status.defaultDetail || draft.status.summary;
    return this.failure(draft, draft.status, draft.status.defaultTitle, [], {}, isGeneric, detail);
  }

  private failure(
    draft: ResultDraft,
    status: ResultStatusInfo,
    title: string,
    errors: readonly string[],
    fieldErrors: Readonly<Record<string, readonly string[]>>,
    isGeneric: boolean,
    detail: string | null = null,
  ): SimulatedResult {
    return { isSuccessful: false, status, title, detail, errors, fieldErrors, data: null, isGeneric };
  }

  private successTitle(draft: ResultDraft): string {
    const { status } = draft;
    switch (status.factory) {
      case 'title':
        return draft.title.trim() || status.defaultTitle;
      case 'location':
      case 'detailAsTitle':
        return status.defaultTitle.replace('{0}', draft.argument);
      default:
        return status.defaultTitle;
    }
  }

  private unprocessable(fallback: ResultStatusInfo): ResultStatusInfo {
    return findStatus('UnprocessableContent') ?? fallback;
  }
}
