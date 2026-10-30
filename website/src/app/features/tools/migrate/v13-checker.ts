export type Certainty = 'fix' | 'review';

export interface Finding {
  readonly line: number;
  readonly certainty: Certainty;
  readonly reason: string;
  readonly before: string;
  readonly after: string;
}

export interface CheckResult {
  readonly findings: readonly Finding[];
  readonly fixed: string;
}

const CONCRETE_GENERIC = /\b(?:Success|Error)DataResult</g;
const CONCRETE_PLAIN = /\b(?:SuccessResult|ErrorResult)\b/g;
const DECLARED_FROM_FACADE_PLAIN = /\b(?:SuccessResult|ErrorResult)(\??)(?=\s+\w+\s*=[^;]*?\bResult\.)/g;
const DECLARED_FROM_FACADE_GENERIC = /\b(?:Success|Error)DataResult<(.+?)>(\??)(?=\s+\w+\s*=[^;]*?\bResult\.)/g;
const CAST = /\((?:SuccessResult|ErrorResult)\)\s*/g;
const METHOD_RETURN = /^(\s*(?:(?:public|private|protected|internal|static|async|sealed|override|virtual|new)\s+)*)((?:Task<|ValueTask<)?(?:SuccessResult|ErrorResult|(?:Success|Error)DataResult<.+?>)\??>?)(\s+\w+\s*(?:<[^>]*>)?\s*\()/;

export const V13_SAMPLE = `public ErrorResult? ValidateCreate(CreateProductRequest request)
{
    ErrorResult? nameCheck = string.IsNullOrEmpty(request.Name) ? Result.Invalid("Name is required.") : null;
    SuccessDataResult<ProductDto> ok = Result.Success(ToDto(product));
    var combined = Result.Combine(nameCheck ?? Result.Success(), Result.Success());
    return combined.IsSuccessful ? null : (ErrorResult)combined;
}

Func<string, ErrorResult> conflict = Result.Conflict;
var unchanged = new ErrorResult("Not found.", ResultStatus.NotFound);`;

export function toBaseTypes(text: string): string {
  return text.replace(CONCRETE_GENERIC, 'OperationDataResult<').replace(CONCRETE_PLAIN, 'OperationResult');
}

export function checkV13(source: string): CheckResult {
  const findings: Finding[] = [];
  const lines = source.replace(/\r\n/g, '\n').split('\n');

  const fixedLines = lines.map((line, index) => {
    let fixed = line
      .replace(DECLARED_FROM_FACADE_GENERIC, (_, typeArgument: string, nullable: string) => `OperationDataResult<${typeArgument}>${nullable}`)
      .replace(DECLARED_FROM_FACADE_PLAIN, (_, nullable: string) => `OperationResult${nullable}`);
    fixed = fixDelegateTypeArguments(fixed);

    if (fixed !== line) {
      findings.push({ line: index + 1, certainty: 'fix', reason: 'Declared as a concrete subclass but assigned from the facade, which now returns the base type.', before: line.trim(), after: fixed.trim() });
    }

    const method = METHOD_RETURN.exec(fixed);
    if (method) {
      const suggestion = method[1] + toBaseTypes(method[2]) + method[3];
      findings.push({ line: index + 1, certainty: 'review', reason: 'Breaks only if the method returns a Result.X(...) call. Methods that return new ErrorResult(...) and similar keep compiling.', before: fixed.trim(), after: (suggestion + fixed.slice(method[0].length)).trim() });
    }

    if (CAST.test(fixed)) {
      CAST.lastIndex = 0;
      findings.push({ line: index + 1, certainty: 'review', reason: 'A cast to a concrete subclass fails at runtime when the value is not that subclass (for example a field-error result from Result.Combine). Usually the cast can simply be removed.', before: fixed.trim(), after: fixed.replace(CAST, '').trim() });
    }
    CAST.lastIndex = 0;

    return fixed;
  });

  return { findings, fixed: fixedLines.join('\n') };
}

function fixDelegateTypeArguments(line: string): string {
  let result = '';
  let cursor = 0;
  const pattern = /\b(?:Func|Expression<Func)</g;
  let match: RegExpExecArray | null;

  while ((match = pattern.exec(line)) !== null) {
    const start = match.index + match[0].length;
    const end = closingBracket(line, start);
    if (end < 0) {
      break;
    }
    result += line.slice(cursor, start) + toBaseTypes(line.slice(start, end));
    cursor = end;
    pattern.lastIndex = end;
  }

  return result + line.slice(cursor);
}

function closingBracket(text: string, from: number): number {
  let depth = 1;
  for (let index = from; index < text.length; index++) {
    if (text[index] === '<') {
      depth++;
    } else if (text[index] === '>') {
      depth--;
      if (depth === 0) {
        return index;
      }
    }
  }
  return -1;
}
