import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TreeNode } from '@core/domain/content.model';

const ICONS: readonly (readonly [RegExp, string])[] = [
  [/\.cs$/, 'bi-filetype-cs'],
  [/\.json$/, 'bi-filetype-json'],
  [/\.(csproj|props|xml|sln|slnx)$/, 'bi-filetype-xml'],
  [/\.(ts|tsx)$/, 'bi-filetype-tsx'],
  [/\.md$/, 'bi-file-earmark-text'],
];

@Component({
  selector: 'rh-file-tree',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ul [class.root]="root()" role="tree">
      @for (node of nodes(); track node.name) {
        <li role="treeitem" [class.hl]="node.highlight">
          @if (node.children) {
            <details open>
              <summary><i class="bi bi-folder2-open folder"></i><span class="name">{{ node.name }}</span>@if (node.note) { <span class="note">{{ node.note }}</span> }</summary>
              <rh-file-tree [nodes]="node.children" [root]="false" />
            </details>
          } @else {
            <div class="file"><i class="bi" [class]="icon(node.name)"></i><span class="name">{{ node.name }}</span>@if (node.note) { <span class="note">{{ node.note }}</span> }</div>
          }
        </li>
      }
    </ul>
  `,
  styles: `
    :host { display: block; }
    ul { list-style: none; margin: 0; padding-left: 1.1rem; border-left: 1px dashed var(--rh-border); }
    ul.root { padding: .85rem 1rem; border: 1px solid var(--rh-border); border-radius: var(--bs-border-radius-lg); background: var(--rh-code-bg); font-family: var(--bs-font-monospace); font-size: .84rem; }
    li { margin: .1rem 0; }
    summary { list-style: none; cursor: pointer; display: flex; align-items: baseline; gap: .45rem; padding: .1rem .3rem; border-radius: .35rem; }
    summary::-webkit-details-marker { display: none; }
    summary:hover { background: var(--rh-surface-2); }
    .file { display: flex; align-items: baseline; gap: .45rem; padding: .1rem .3rem; }
    .folder { color: var(--rh-warning); }
    .file .bi { color: var(--rh-info); }
    .note { color: var(--rh-muted); font-family: var(--bs-font-sans-serif); font-size: .78rem; }
    .note::before { content: "·"; margin-right: .4rem; }
    li.hl > .file, li.hl > details > summary { background: var(--rh-accent-soft); }
    li.hl > .file .name, li.hl > details > summary .name { color: var(--rh-accent); font-weight: 700; }
  `,
})
export class FileTree {
  readonly nodes = input.required<readonly TreeNode[]>();
  readonly root = input(true);

  protected icon(name: string): string {
    return ICONS.find(([pattern]) => pattern.test(name))?.[1] ?? 'bi-file-earmark-code';
  }
}
