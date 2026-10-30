export type CodeLanguage = 'csharp' | 'json' | 'bash' | 'xml' | 'typescript' | 'powershell' | 'http' | 'plaintext';

export type CalloutTone = 'info' | 'tip' | 'warning' | 'success';

export interface CodeSnippet {
  readonly label?: string;
  readonly language: CodeLanguage;
  readonly code: string;
}

export interface TreeNode {
  readonly name: string;
  readonly note?: string;
  readonly highlight?: boolean;
  readonly children?: readonly TreeNode[];
}

export interface ProcedureStep {
  readonly title: string;
  readonly blocks: readonly ContentBlock[];
}

export type ContentBlock =
  | { readonly kind: 'paragraph'; readonly text: string }
  | { readonly kind: 'code'; readonly snippet: CodeSnippet; readonly caption?: string }
  | { readonly kind: 'tabs'; readonly snippets: readonly CodeSnippet[]; readonly syncKey?: string }
  | { readonly kind: 'list'; readonly items: readonly string[]; readonly ordered?: boolean }
  | { readonly kind: 'table'; readonly head: readonly string[]; readonly rows: readonly (readonly string[])[] }
  | { readonly kind: 'callout'; readonly tone: CalloutTone; readonly title?: string; readonly text: string }
  | { readonly kind: 'cards'; readonly cards: readonly LinkCard[] }
  | { readonly kind: 'tree'; readonly nodes: readonly TreeNode[] }
  | { readonly kind: 'procedure'; readonly steps: readonly ProcedureStep[] };

export interface LinkCard {
  readonly icon: string;
  readonly title: string;
  readonly text: string;
  readonly link?: string;
}

export interface ContentSection {
  readonly id: string;
  readonly title: string;
  readonly blocks: readonly ContentBlock[];
}

export function flattenBlocks(blocks: readonly ContentBlock[]): readonly ContentBlock[] {
  return blocks.flatMap(block => (block.kind === 'procedure' ? [block, ...flattenBlocks(block.steps.flatMap(step => step.blocks))] : [block]));
}
