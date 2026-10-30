import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentBlock } from '@core/domain/content.model';
import { Callout } from './callout';
import { FileTree } from './file-tree';
import { CodeBlock } from './code-block';
import { CodeTabs } from './code-tabs';
import { InlineMarkupPipe } from './inline-markup.pipe';
import { InternalLinksDirective } from './internal-links.directive';

@Component({
  selector: 'rh-content-renderer',
  imports: [Callout, CodeBlock, CodeTabs, FileTree, InlineMarkupPipe, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  hostDirectives: [InternalLinksDirective],
  template: `
    @for (block of blocks(); track $index) {
      @switch (block.kind) {
        @case ('paragraph') {
          <p [innerHTML]="block.text | inlineMarkup"></p>
        }
        @case ('code') {
          <div class="block">
            <rh-code-block [code]="block.snippet.code" [language]="block.snippet.language" [title]="block.snippet.label" />
            @if (block.caption) {
              <p class="caption" [innerHTML]="block.caption | inlineMarkup"></p>
            }
          </div>
        }
        @case ('tabs') {
          <rh-code-tabs class="block" [snippets]="block.snippets" [syncKey]="block.syncKey" />
        }
        @case ('list') {
          @if (block.ordered) {
            <ol class="rh-list">
              @for (item of block.items; track $index) { <li [innerHTML]="item | inlineMarkup"></li> }
            </ol>
          } @else {
            <ul class="rh-list">
              @for (item of block.items; track $index) { <li [innerHTML]="item | inlineMarkup"></li> }
            </ul>
          }
        }
        @case ('table') {
          <div class="table-responsive block surface">
            <table class="table align-middle mb-0">
              <thead>
                <tr>@for (cell of block.head; track $index) { <th scope="col">{{ cell }}</th> }</tr>
              </thead>
              <tbody>
                @for (row of block.rows; track $index) {
                  <tr>@for (cell of row; track $index) { <td [innerHTML]="cell | inlineMarkup"></td> }</tr>
                }
              </tbody>
            </table>
          </div>
        }
        @case ('callout') {
          <rh-callout class="block d-block" [tone]="block.tone" [title]="block.title">
            <span [innerHTML]="block.text | inlineMarkup"></span>
          </rh-callout>
        }
        @case ('tree') {
          <rh-file-tree class="block" [nodes]="block.nodes" />
        }
        @case ('procedure') {
          <ol class="procedure block">
            @for (step of block.steps; track $index) {
              <li class="step">
                <h3 class="step-title">{{ step.title }}</h3>
                <rh-content-renderer [blocks]="step.blocks" />
              </li>
            }
          </ol>
        }
        @case ('cards') {
          <div class="row g-3 block">
            @for (card of block.cards; track card.title) {
              <div class="col-md-6">
                @if (card.link) {
                  <a class="card-link surface surface-hover" [routerLink]="card.link">
                    <span class="icon-tile"><i class="bi" [class]="card.icon"></i></span>
                    <span><strong class="d-block text-body">{{ card.title }}</strong><small class="text-muted-rh">{{ card.text }}</small></span>
                  </a>
                } @else {
                  <div class="card-link surface">
                    <span class="icon-tile"><i class="bi" [class]="card.icon"></i></span>
                    <span><strong class="d-block">{{ card.title }}</strong><small class="text-muted-rh" [innerHTML]="card.text | inlineMarkup"></small></span>
                  </div>
                }
              </div>
            }
          </div>
        }
      }
    }
  `,
  styles: `
    :host { display: block; }
    p { line-height: 1.75; color: var(--rh-text); }
    .block { display: block; margin: 1.25rem 0; }
    .caption { font-size: .85rem; color: var(--rh-muted); margin: .5rem 0 0; }
    .rh-list { line-height: 1.8; padding-left: 1.25rem; }
    .rh-list li::marker { color: var(--rh-accent); }
    th { font-size: .78rem; text-transform: uppercase; letter-spacing: .05em; color: var(--rh-muted); font-weight: 600; }
    td, th { padding: .7rem 1rem; }
    @media (max-width: 575.98px) { td, th { padding: .5rem .6rem; } }
    .procedure { list-style: none; padding: 0; counter-reset: step; }
    .step { position: relative; padding: 0 0 .5rem 3rem; counter-increment: step; }
    .step::before { content: counter(step); position: absolute; left: 0; top: 0; width: 2rem; height: 2rem; display: grid; place-items: center; border-radius: 50%; background: var(--rh-accent-soft); color: var(--rh-accent); font-weight: 800; font-size: .9rem; border: 1px solid color-mix(in srgb, var(--rh-accent) 40%, transparent); }
    .step::after { content: ""; position: absolute; left: 1rem; top: 2.25rem; bottom: .25rem; width: 2px; background: var(--rh-border); }
    .step:last-child::after { display: none; }
    .step-title { font-size: 1.05rem; font-weight: 700; margin: .3rem 0 .5rem; }
    .step rh-content-renderer ::ng-deep .block:first-child { margin-top: .5rem; }
    .card-link { display: flex; gap: .9rem; align-items: flex-start; padding: 1rem; height: 100%; color: inherit; }
  `,
})
export class ContentRenderer {
  readonly blocks = input.required<readonly ContentBlock[]>();
}
