import { ContentBlock, TreeNode } from '@core/domain/content.model';
import { DocPage } from '../domain/doc-page';

const BLANK_LINE = '\n\n';

export class MarkdownExporter {
  export(page: DocPage, url: string): string {
    const parts = [`# ${page.title}`, page.summary, `Source: ${url}`];
    for (const section of page.sections) {
      parts.push(`## ${section.title}`);
      parts.push(...section.blocks.map(block => this.block(block)));
    }
    return `${parts.filter(Boolean).join(BLANK_LINE)}\n`;
  }

  private block(block: ContentBlock): string {
    switch (block.kind) {
      case 'paragraph':
        return block.text;
      case 'code':
        return [block.snippet.label ? `**${block.snippet.label}**` : '', fence(block.snippet.language, block.snippet.code), block.caption ?? '']
          .filter(Boolean)
          .join(BLANK_LINE);
      case 'tabs':
        return block.snippets.map(snippet => `**${snippet.label ?? snippet.language}**${BLANK_LINE}${fence(snippet.language, snippet.code)}`).join(BLANK_LINE);
      case 'list':
        return block.items.map((item, index) => `${block.ordered ? `${index + 1}.` : '-'} ${item}`).join('\n');
      case 'table':
        return [row(block.head), row(block.head.map(() => '---')), ...block.rows.map(row)].join('\n');
      case 'callout':
        return `> **${block.title ?? capitalize(block.tone)}:** ${block.text}`;
      case 'tree':
        return fence('plaintext', treeText(block.nodes, ''));
      case 'procedure':
        return block.steps
          .map((step, index) => `### ${index + 1}. ${step.title}${BLANK_LINE}${step.blocks.map(inner => this.block(inner)).join(BLANK_LINE)}`)
          .join(BLANK_LINE);
      case 'cards':
        return block.cards.map(card => `- **${card.title}**: ${card.text}${card.link ? ` (${card.link})` : ''}`).join('\n');
    }
  }
}

function treeText(nodes: readonly TreeNode[], indent: string): string {
  return nodes
    .map(node => {
      const line = `${indent}${node.children ? `${node.name}/` : node.name}${node.note ? `  # ${node.note}` : ''}`;
      return node.children ? `${line}\n${treeText(node.children, `${indent}  `)}` : line;
    })
    .join('\n');
}

function fence(language: string, code: string): string {
  return `\`\`\`${language === 'plaintext' ? '' : language}\n${code}\n\`\`\``;
}

function row(cells: readonly string[]): string {
  return `| ${cells.map(cell => cell.replaceAll('|', '\\|')).join(' | ')} |`;
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}
