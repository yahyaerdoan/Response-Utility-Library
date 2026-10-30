import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const dataDir = join(root, 'src/app/features/docs/data');
const files = ['getting-started.ts', 'fundamentals.ts', 'aspnetcore.ts', 'composition.ts', 'advanced.ts', 'guides.ts', 'reference.ts'];

const pages = new Map();
for (const file of files) {
  const text = readFileSync(join(dataDir, file), 'utf8');
  for (const match of text.matchAll(/export const (\w+): DocPage = \{\s*slug: '([^']+)',\s*title: '([^']+)',[\s\S]*?summary: '([^']+)'/g)) {
    pages.set(match[1], { slug: match[2], title: match[3], summary: match[4] });
  }
}

const catalog = readFileSync(join(dataDir, 'docs.catalog.ts'), 'utf8');
const lines = [
  '# ResponseResultHandler',
  '',
  '> Result-pattern library for .NET 8, 9 and 10. Wraps an operation outcome (success or failure, an HTTP-mappable ResultStatus, title, detail, errors, per-field errors and optional data) in an immutable object, and converts it to IActionResult / IResult with RFC 9457 Problem Details.',
  '',
  'Packages: ResponseResultHandler (core, BCL only) and ResponseResultHandler.AspNetCore (MVC and Minimal API adapters).',
  '',
];

for (const group of catalog.matchAll(/title: '([^']+)', pages: \[([^\]]+)\]/g)) {
  lines.push(`## ${group[1]}`, '');
  for (const name of group[2].split(',').map(item => item.trim())) {
    const page = pages.get(name);
    if (!page) {
      throw new Error(`Page constant ${name} was not found in the docs data files.`);
    }
    lines.push(`- [${page.title}](/docs/${page.slug}): ${page.summary}`);
  }
  lines.push('');
}

lines.push(
  '## Tools',
  '',
  '- [Status explorer](/statuses): every ResultStatus with its factory, defaults and Problem Details type',
  '- [Playground](/playground): build a result and see the C# call, HTTP response and a unit test',
  '- [Pipeline](/pipeline): Map / Bind / Ensure short-circuiting step by step',
  '',
);

writeFileSync(join(root, 'public/llms.txt'), lines.join('\n'));
console.log(`llms.txt written with ${pages.size} pages.`);
