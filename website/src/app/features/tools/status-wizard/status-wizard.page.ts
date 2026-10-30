import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';
import { PageHeader } from '@shared/ui/page-header';
import { WIZARD_ROOT, WizardNode, WizardQuestion } from './status-wizard.domain';

interface Answer {
  readonly question: WizardQuestion;
  readonly yes: boolean;
}

@Component({
  selector: 'rh-status-wizard-page',
  imports: [RouterLink, PageHeader, CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Tools" icon="bi-signpost-split" title="Which status should I return?"
                    lead="Answer a few yes/no questions and get the status, the facade call and the reason behind it." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="row g-4">
        <div class="col-lg-7">
          @if (answers().length > 0) {
            <ol class="trail list-unstyled mb-4">
              @for (answer of answers(); track answer.question.id; let index = $index) {
                <li>
                  <button type="button" class="trail-item" (click)="rewind(index)" [attr.aria-label]="'Change answer: ' + answer.question.text">
                    <span class="small text-muted-rh">{{ answer.question.text }}</span>
                    <span class="badge" [class.yes]="answer.yes">{{ answer.yes ? 'Yes' : 'No' }}</span>
                  </button>
                </li>
              }
            </ol>
          }

          @let node = current();
          @if (node.kind === 'question') {
            <section class="surface p-4" aria-live="polite">
              <span class="eyebrow">Question {{ answers().length + 1 }}</span>
              <h2 class="h4 fw-bold mt-1">{{ node.text }}</h2>
              @if (node.help) {
                <p class="text-muted-rh small mb-0">{{ node.help }}</p>
              }
              <div class="d-flex gap-2 mt-4">
                <button type="button" class="btn btn-primary px-4" (click)="answer(true)">Yes</button>
                <button type="button" class="btn btn-outline-secondary px-4" (click)="answer(false)">No</button>
              </div>
            </section>
          } @else {
            <section class="surface p-4 result" aria-live="polite">
              <span class="eyebrow">Suggested status</span>
              <div class="d-flex align-items-baseline gap-3 mt-1 flex-wrap">
                <span class="code">{{ node.code }}</span>
                <h2 class="h3 fw-bold mb-0">{{ node.status }}</h2>
              </div>
              <p class="mt-3 mb-3">{{ node.why }}</p>
              <rh-code-block [code]="node.snippet" language="csharp" />
              @if (node.note) {
                <p class="small text-muted-rh mt-3 mb-0"><i class="bi bi-info-circle me-1"></i>{{ node.note }}</p>
              }
              <div class="d-flex gap-2 mt-4 flex-wrap">
                <a class="btn btn-outline-primary btn-sm" routerLink="/statuses" [queryParams]="{ s: node.status }">Open in status explorer</a>
                <button type="button" class="btn btn-outline-secondary btn-sm" (click)="restart()">Start over</button>
              </div>
            </section>
          }
        </div>

        <aside class="col-lg-5">
          <div class="surface p-4">
            <h2 class="h6 fw-bold">The short version</h2>
            <ul class="small mb-0 ps-3">
              <li><strong>404</strong>: it does not exist.</li>
              <li><strong>409</strong>: it exists, but its current state does not allow this.</li>
              <li><strong>422</strong>: the request content is invalid on its own.</li>
              <li><strong>502</strong>: a provider you depend on failed.</li>
              <li><strong>500</strong>: your own server-side step failed.</li>
              <li><strong>400</strong>: keep it for malformed requests and failed sign-in flows.</li>
            </ul>
            <p class="small text-muted-rh mt-3 mb-0">More in <a routerLink="/docs/pitfalls" fragment="bad-request">Common pitfalls</a>.</p>
          </div>
        </aside>
      </div>
    </div>
  `,
  styles: `
    .trail { display: grid; gap: .4rem; }
    .trail-item { display: flex; justify-content: space-between; align-items: center; gap: 1rem; width: 100%; padding: .5rem .75rem; border: 1px solid var(--rh-border); border-radius: .6rem; background: transparent; text-align: start; color: inherit; }
    .trail-item:hover, .trail-item:focus-visible { background: var(--rh-surface-2); }
    .badge { background: var(--rh-surface-2); color: var(--rh-muted); font-weight: 600; }
    .badge.yes { background: var(--rh-accent-soft); color: var(--rh-accent); }
    .code { font-size: 2.5rem; font-weight: 800; line-height: 1; color: var(--rh-accent); font-variant-numeric: tabular-nums; }
  `,
})
export class StatusWizardPage {
  protected readonly answers = signal<readonly Answer[]>([]);

  protected readonly current = computed<WizardNode>(() => {
    let node: WizardNode = WIZARD_ROOT;
    for (const answer of this.answers()) {
      node = answer.yes ? answer.question.yes : answer.question.no;
    }
    return node;
  });

  protected answer(yes: boolean): void {
    const node = this.current();
    if (node.kind === 'question') {
      this.answers.update(answers => [...answers, { question: node, yes }]);
    }
  }

  protected rewind(index: number): void {
    this.answers.update(answers => answers.slice(0, index));
  }

  protected restart(): void {
    this.answers.set([]);
  }
}
