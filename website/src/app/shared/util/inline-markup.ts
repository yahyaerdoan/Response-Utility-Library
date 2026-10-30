import { escapeHtml } from './highlighter';

const CODE = /`([^`]+)`/g;
const BOLD = /\*\*([^*]+)\*\*/g;
const LINK = /\[([^\]]+)\]\(([^)\s]+)\)/g;
const PLACEHOLDER = /\u0000(\d+)\u0000/g;

export function renderInlineMarkup(source: string): string {
  const codeSpans: string[] = [];
  const withoutCode = escapeHtml(source).replace(CODE, (_, code: string) => {
    codeSpans.push(`<code>${code}</code>`);
    return `\u0000${codeSpans.length - 1}\u0000`;
  });
  return withoutCode
    .replace(BOLD, '<strong>$1</strong>')
    .replace(LINK, (_, text: string, href: string) => linkHtml(text, href))
    .replace(PLACEHOLDER, (_, index: string) => codeSpans[Number(index)] ?? '');
}

function linkHtml(text: string, href: string): string {
  const external = /^https?:\/\//.test(href);
  const attributes = external ? ' target="_blank" rel="noopener"' : '';
  return `<a href="${href}"${attributes}>${text}</a>`;
}
