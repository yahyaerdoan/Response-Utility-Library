import { factoryName } from '@shared/kernel/result-status';
import { ResultBuilder } from './result-builder';
import { ResultDraft } from './result-draft';

export class CSharpGenerator {
  constructor(private readonly builder: ResultBuilder) {}

  generate(draft: ResultDraft): string {
    switch (this.builder.mode(draft)) {
      case 'fieldErrors':
        return this.fieldErrors(draft);
      case 'invalid':
        return this.invalid(draft);
      case 'errorList':
        return this.errorList(draft);
      case 'facade':
        return draft.status.isSuccessful ? this.success(draft) : this.failure(draft);
    }
  }

  private success(draft: ResultDraft): string {
    const { status } = draft;
    const generic = this.builder.isGeneric(draft);
    const name = factoryName(status);
    const args: string[] = [];
    if (generic) {
      args.push('product');
    }
    if (status.factory === 'title' && draft.title.trim()) {
      args.push(quote(draft.title.trim()));
    }
    if (status.factory === 'location' || status.factory === 'detailAsTitle') {
      args.push(quote(draft.argument));
    }
    const type = generic ? `IOperationResult<ProductDto>` : 'IOperationResult';
    const productLine = generic ? `${this.productDeclaration(draft)}\n` : '';
    return `using ResultHandler.Facade;\n\n${productLine}${type} result = Result.${name}(${args.join(', ')});`;
  }

  private failure(draft: ResultDraft): string {
    const { status } = draft;
    const generic = this.builder.isGeneric(draft);
    const detail = draft.detail.trim();
    const typeArgument = generic ? '<ProductDto>' : '';
    const argument = detail || status.factory === 'detail' ? quote(detail || status.summary) : '';
    const type = generic ? 'IOperationResult<ProductDto>' : 'IOperationResult';
    return `using ResultHandler.Facade;\n\n${type} result = Result.${status.name}${typeArgument}(${argument});`;
  }

  private invalid(draft: ResultDraft): string {
    const generic = this.builder.isGeneric(draft);
    const typeArgument = generic ? '<ProductDto>' : '';
    const errors = draft.errors.map(quote).join(',\n    ');
    return `using ResultHandler.Facade;\n\nvar result = Result.Invalid${typeArgument}(\n    ${errors});`;
  }

  private errorList(draft: ResultDraft): string {
    const generic = this.builder.isGeneric(draft);
    const type = generic ? 'ErrorDataResult<ProductDto>' : 'ErrorResult';
    const errors = draft.errors.map(quote).join(', ');
    return [
      'using ResultHandler.Core.Enums;',
      'using ResultHandler.Implementations.Error;',
      '',
      `var result = new ${type}(`,
      `    ${quote(draft.status.defaultTitle)},`,
      `    ResultStatus.${draft.status.name},`,
      `    new[] { ${errors} });`,
    ].join('\n');
  }

  private fieldErrors(draft: ResultDraft): string {
    const generic = this.builder.isGeneric(draft);
    const type = generic ? 'OperationDataResult<ProductDto>' : 'OperationResult';
    const entries = Object.entries(draft.fieldErrors)
      .map(([field, messages]) => `    [${quote(field)}] = [${messages.map(quote).join(', ')}],`)
      .join('\n');
    return [
      'using ResultHandler.Core.Base;',
      '',
      'var fieldErrors = new Dictionary<string, IReadOnlyList<string>>',
      '{',
      entries,
      '};',
      '',
      `${type} result = ${type}.Failure(fieldErrors);`,
    ].join('\n');
  }

  private productDeclaration(draft: ResultDraft): string {
    const { id, name, price } = draft.product;
    return `var product = new ProductDto(${id}, ${quote(name)}, ${price}m);`;
  }
}

function quote(value: string): string {
  return `"${value.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;
}
