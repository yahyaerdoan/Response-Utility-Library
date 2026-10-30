import { isBodyless, reasonPhrase } from '@shared/kernel/result-status';
import { ResultDraft, SimulatedResult } from './result-draft';

export interface HttpResponseView {
  readonly statusLine: string;
  readonly contentType: string | null;
  readonly body: string | null;
  readonly note: string;
}

export interface ResponseAdapter {
  readonly id: string;
  readonly label: string;
  readonly icon: string;
  methodName(result: SimulatedResult): string;
  render(result: SimulatedResult, draft: ResultDraft): HttpResponseView;
}

const PROBLEM_JSON = 'application/problem+json';
const JSON_TYPE = 'application/json; charset=utf-8';

function statusLine(result: SimulatedResult): string {
  return `HTTP/1.1 ${result.status.code} ${reasonPhrase(result.status)}`;
}

function pretty(value: unknown): string {
  return JSON.stringify(value, null, 2);
}

export function toSerializedEnvelope(result: SimulatedResult): Record<string, unknown> {
  const envelope: Record<string, unknown> = {};
  if (result.isGeneric) {
    envelope['resultData'] = result.data;
  }
  envelope['isSuccessful'] = result.isSuccessful;
  envelope['statusCode'] = result.status.code;
  envelope['statusMessage'] = result.title;
  if (result.detail !== null) {
    envelope['detail'] = result.detail;
  }
  envelope['errors'] = result.errors;
  envelope['fieldErrors'] = result.fieldErrors;
  return envelope;
}

export function toProblemDetails(result: SimulatedResult, draft: ResultDraft): Record<string, unknown> {
  const problem: Record<string, unknown> = {
    type: result.status.problemType,
    title: result.title,
    status: result.status.code,
  };
  if (result.detail !== null) {
    problem['detail'] = result.detail;
  }
  if (draft.passHttpContext) {
    problem['instance'] = draft.requestPath;
  }
  if (result.errors.length > 0) {
    problem['errors'] = result.errors;
  }
  if (Object.keys(result.fieldErrors).length > 0) {
    problem['fieldErrors'] = result.fieldErrors;
  }
  return problem;
}

abstract class HttpAdapter implements ResponseAdapter {
  abstract readonly id: string;
  abstract readonly label: string;
  abstract readonly icon: string;
  abstract methodName(result: SimulatedResult): string;
  protected abstract successBody(result: SimulatedResult): unknown;
  protected abstract successNote(result: SimulatedResult): string;

  render(result: SimulatedResult, draft: ResultDraft): HttpResponseView {
    if (!result.isSuccessful) {
      return {
        statusLine: statusLine(result),
        contentType: PROBLEM_JSON,
        body: pretty(toProblemDetails(result, draft)),
        note: `Failures always become an RFC 9457 Problem Details body. \`type\` links to the RFC section that defines ${result.status.code}.`,
      };
    }
    const body = isBodyless(result.status) ? undefined : this.successBody(result);
    if (body === undefined) {
      return {
        statusLine: statusLine(result),
        contentType: null,
        body: null,
        note: isBodyless(result.status)
          ? `${result.status.code} is bodyless by definition (1xx, 3xx, 204 and 304 never carry a payload).`
          : this.successNote(result),
      };
    }
    return { statusLine: statusLine(result), contentType: JSON_TYPE, body: pretty(body), note: this.successNote(result) };
  }
}

export class PayloadAdapter extends HttpAdapter {
  readonly id = 'payload';
  readonly label = 'Payload';
  readonly icon = 'bi-box-seam';

  methodName(result: SimulatedResult): string {
    return result.isGeneric ? 'ToActionResult<T>()  ·  ToResult<T>()' : 'ToActionResult()  ·  ToResult()';
  }

  protected successBody(result: SimulatedResult): unknown {
    return result.isGeneric ? result.data : undefined;
  }

  protected successNote(result: SimulatedResult): string {
    return result.isGeneric
      ? `Only the raw \`T\` is returned, with the result's real status (${result.status.code}), not a hard-coded 200.`
      : 'Non-generic results are commands: success is always a bodyless status code.';
  }
}

export class EnvelopeAdapter extends HttpAdapter {
  readonly id = 'envelope';
  readonly label = 'Envelope';
  readonly icon = 'bi-envelope-paper';

  methodName(): string {
    return 'ToEnvelopedActionResult()  ·  ToEnvelopedResult()';
  }

  protected successBody(result: SimulatedResult): unknown {
    return toSerializedEnvelope(result);
  }

  protected successNote(): string {
    return 'The whole result (status, title, data) is the body. Useful when clients want metadata next to the data.';
  }
}

export class SerializerAdapter implements ResponseAdapter {
  readonly id = 'serializer';
  readonly label = 'JsonSerializer';
  readonly icon = 'bi-braces';

  methodName(): string {
    return 'JsonSerializer.Serialize(result)';
  }

  render(result: SimulatedResult): HttpResponseView {
    return {
      statusLine: 'System.Text.Json output',
      contentType: null,
      body: pretty(toSerializedEnvelope(result)),
      note: 'Property names are fixed regardless of your naming policy, `statusCode` is always numeric, `detail` is omitted when null, `errors`/`fieldErrors` are always present.',
    };
  }
}

export const RESPONSE_ADAPTERS: readonly ResponseAdapter[] = [new PayloadAdapter(), new EnvelopeAdapter(), new SerializerAdapter()];
