import { SimulatedResult } from './result-draft';

export class AssertionGenerator {
  generate(result: SimulatedResult): string {
    const lines = [
      `Assert.${result.isSuccessful ? 'True' : 'False'}(result.IsSuccessful);`,
      `Assert.Equal(ResultStatus.${result.status.name}, result.Status);`,
      `Assert.Equal(${literal(result.title)}, result.Title);`,
    ];
    if (result.detail !== null) {
      lines.push(`Assert.Equal(${literal(result.detail)}, result.Detail);`);
    }
    if (result.errors.length > 0) {
      lines.push(`Assert.Equal(new[] { ${result.errors.map(literal).join(', ')} }, result.Errors);`);
    }
    for (const field of Object.keys(result.fieldErrors)) {
      lines.push(`Assert.Contains(${literal(field)}, result.GetFieldErrors().Keys);`);
    }
    if (result.isSuccessful && result.data) {
      lines.push(`Assert.Equal(${result.data.id}, result.Data.Id);`);
    }
    return `[Fact]\npublic void Returns_${result.status.name}()\n{\n    var result = Sut.Execute();\n\n    ${lines.join('\n    ')}\n}`;
  }
}

function literal(value: string): string {
  return `"${value.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;
}
