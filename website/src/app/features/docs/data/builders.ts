import { CalloutTone, CodeLanguage, CodeSnippet, ContentBlock, ContentSection, LinkCard, ProcedureStep, TreeNode } from '@core/domain/content.model';

export const section = (id: string, title: string, ...blocks: ContentBlock[]): ContentSection => ({ id, title, blocks });

export const p = (text: string): ContentBlock => ({ kind: 'paragraph', text });

export const code = (language: CodeLanguage, source: string, label?: string, caption?: string): ContentBlock => ({
  kind: 'code',
  snippet: { language, code: source.trim(), label },
  caption,
});

export const cs = (source: string, label?: string, caption?: string): ContentBlock => code('csharp', source, label, caption);

export const json = (source: string, label?: string): ContentBlock => code('json', source, label);

export const tab = (label: string, language: CodeLanguage, source: string): CodeSnippet => ({ label, language, code: source.trim() });

export const tabs = (...snippets: CodeSnippet[]): ContentBlock => ({ kind: 'tabs', snippets });

export const syncedTabs = (syncKey: string, ...snippets: CodeSnippet[]): ContentBlock => ({ kind: 'tabs', snippets, syncKey });

export const minimalApi = (source: string): CodeSnippet => tab('Minimal API', 'csharp', source);

export const mvcController = (source: string): CodeSnippet => tab('MVC controller', 'csharp', source);

export const list = (...items: string[]): ContentBlock => ({ kind: 'list', items });

export const steps = (...items: string[]): ContentBlock => ({ kind: 'list', items, ordered: true });

export const table = (head: readonly string[], ...rows: (readonly string[])[]): ContentBlock => ({ kind: 'table', head, rows });

const callout = (tone: CalloutTone) => (text: string, title?: string): ContentBlock => ({ kind: 'callout', tone, text, title });

export const note = callout('info');
export const tip = callout('tip');
export const warn = callout('warning');
export const good = callout('success');

export const cards = (...items: LinkCard[]): ContentBlock => ({ kind: 'cards', cards: items });

export const tree = (...nodes: TreeNode[]): ContentBlock => ({ kind: 'tree', nodes });

export const dir = (name: string, note: string | null, ...children: TreeNode[]): TreeNode => ({ name, note: note ?? undefined, children });

export const file = (name: string, note?: string, highlight = false): TreeNode => ({ name, note, highlight });

export const procedure = (...steps: ProcedureStep[]): ContentBlock => ({ kind: 'procedure', steps });

export const step = (title: string, ...blocks: ContentBlock[]): ProcedureStep => ({ title, blocks });
