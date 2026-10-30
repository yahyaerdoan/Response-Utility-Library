import { factoryName, isBodyless, ResultStatusInfo } from '@shared/kernel/result-status';

export interface StatusSignatures {
  readonly plain: string;
  readonly generic: string | null;
  readonly failureFactory: string | null;
}

export function signaturesOf(status: ResultStatusInfo): StatusSignatures {
  const name = factoryName(status);
  if (status.isSuccessful) {
    const parameter = successParameter(status);
    return {
      plain: `OperationResult Result.${name}(${parameter})`,
      generic: status.hasGenericFactory ? `OperationDataResult<T> Result.${name}<T>(T data${parameter ? ', ' + parameter : ''})` : null,
      failureFactory: null,
    };
  }
  const parameter = status.factory === 'detail' ? 'string detail' : 'string detail = "…"';
  return {
    plain: `OperationResult Result.${name}(${parameter})`,
    generic: `OperationDataResult<T> Result.${name}<T>(${parameter})`,
    failureFactory: `TSelf ResultFailureFactory.${name}<TSelf>(${parameter})`,
  };
}

export function responseBehaviour(status: ResultStatusInfo): string {
  if (!status.isSuccessful) {
    return `RFC 9457 Problem Details (\`application/problem+json\`) with \`status: ${status.code}\`.`;
  }
  if (isBodyless(status)) {
    return `Bodyless ${status.code}: 1xx, 3xx, 204 and 304 never carry a payload.`;
  }
  return `Generic: raw \`T\` with ${status.code}. Non-generic: bodyless ${status.code}. Enveloped: the full result object.`;
}

export function sampleProblem(status: ResultStatusInfo): string {
  return JSON.stringify(
    {
      type: status.problemType,
      title: status.defaultTitle,
      status: status.code,
      detail: status.defaultDetail ?? 'Your detail message.',
      instance: '/api/products/42',
    },
    null,
    2,
  );
}

function successParameter(status: ResultStatusInfo): string {
  switch (status.factory) {
    case 'title':
      return `string title = "${status.defaultTitle}"`;
    case 'location':
      return `string ${status.locationArgument ?? 'location'}`;
    case 'detailAsTitle':
      return 'string detail';
    default:
      return '';
  }
}
