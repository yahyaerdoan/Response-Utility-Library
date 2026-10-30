export type TerminalLineKind = 'command' | 'output' | 'json' | 'comment';

export interface TerminalLine {
  readonly kind: TerminalLineKind;
  readonly text: string;
}

export const TERMINAL_SCRIPT: readonly TerminalLine[] = [
  { kind: 'comment', text: '# 1. add the packages' },
  { kind: 'command', text: 'dotnet add package ResponseResultHandler.AspNetCore' },
  { kind: 'output', text: "info : PackageReference for 'ResponseResultHandler.AspNetCore' added to 'Shop.Api.csproj'." },
  { kind: 'comment', text: '# 2. run the API' },
  { kind: 'command', text: 'dotnet run --project Shop.Api' },
  { kind: 'output', text: 'info: Microsoft.Hosting.Lifetime[14] Now listening on: https://localhost:7188' },
  { kind: 'comment', text: '# 3. ask for something that does not exist' },
  { kind: 'command', text: 'curl -s https://localhost:7188/api/products/999' },
  {
    kind: 'json',
    text: `{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Product 999 does not exist.",
  "instance": "/api/products/999"
}`,
  },
  { kind: 'comment', text: '# 4. send an invalid product' },
  { kind: 'command', text: `curl -s -X POST https://localhost:7188/api/products -H "Content-Type: application/json" -d '{"name":"","price":0}'` },
  {
    kind: 'json',
    text: `{
  "title": "Validation Failed",
  "status": 422,
  "fieldErrors": {
    "Name": ["Name is required."],
    "Price": ["Price must be greater than zero."]
  }
}`,
  },
];
