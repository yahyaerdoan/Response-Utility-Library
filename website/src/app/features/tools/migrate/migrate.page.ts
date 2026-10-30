import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CodeBlock } from '@shared/ui/code-block';
import { PageHeader } from '@shared/ui/page-header';
import { checkV13, V13_SAMPLE } from './v13-checker';

@Component({
  selector: 'rh-migrate-page',
  imports: [RouterLink, PageHeader, CodeBlock],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Tools" icon="bi-arrow-up-circle" title="v13 upgrade checker"
                    lead="Paste C# code that uses the Result facade. See which lines break in v13 and get the fixed version. Everything runs in your browser." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="row g-4">
        <div class="col-lg-6">
          <label class="form-label fw-semibold" for="v13-source">Your code</label>
          <textarea id="v13-source" class="form-control font-monospace source" spellcheck="false"
                    [value]="source()" (input)="source.set($any($event.target).value)"></textarea>
          <div class="d-flex gap-2 mt-2">
            <button type="button" class="btn btn-outline-secondary btn-sm" (click)="source.set(sample)">Load sample</button>
            <button type="button" class="btn btn-outline-secondary btn-sm" (click)="source.set('')">Clear</button>
          </div>

          @let result = check();
          <h2 class="h6 fw-bold mt-4">
            Findings
            <span class="badge ms-1">{{ fixCount() }} fixed</span>
            <span class="badge review ms-1">{{ reviewCount() }} to review</span>
          </h2>
          @if (result.findings.length === 0) {
            <p class="small text-muted-rh">Nothing that breaks in v13. Code that uses <code>var</code>, the interfaces or constructors keeps compiling.</p>
          } @else {
            <ul class="list-unstyled findings">
              @for (finding of result.findings; track $index) {
                <li class="finding" [class.review]="finding.certainty === 'review'">
                  <div class="small fw-semibold">
                    Line {{ finding.line }}
                    <span class="kind">{{ finding.certainty === 'fix' ? 'fixed' : 'review' }}</span>
                  </div>
                  <p class="small text-muted-rh mb-1">{{ finding.reason }}</p>
                  <code class="d-block small after">{{ finding.after }}</code>
                </li>
              }
            </ul>
          }
        </div>

        <div class="col-lg-6">
          <rh-code-block [code]="result.fixed || '// paste code on the left'" language="csharp" title="Fixed code" />
          <p class="small text-muted-rh mt-3 mb-0">
            Only certain fixes are applied automatically. Lines marked <em>review</em> are suggestions: they break only in some cases.
            Full details: <a routerLink="/docs/migration-v13">Migrating to v13</a>.
          </p>
        </div>
      </div>
    </div>
  `,
  styles: `
    .source { min-height: 18rem; font-size: .85rem; }
    .badge { background: var(--rh-accent-soft); color: var(--rh-accent); font-weight: 600; }
    .badge.review { background: color-mix(in srgb, var(--rh-warning) 15%, transparent); color: var(--rh-warning); }
    .findings { display: grid; gap: .5rem; }
    .finding { padding: .6rem .75rem; border-left: 3px solid var(--rh-accent); background: var(--rh-surface-2); border-radius: .4rem; }
    .finding.review { border-left-color: var(--rh-warning); }
    .kind { margin-left: .5rem; font-size: .7rem; text-transform: uppercase; letter-spacing: .08em; color: var(--rh-muted); }
    .after { white-space: pre-wrap; word-break: break-word; }
  `,
})
export class MigratePage {
  protected readonly sample = V13_SAMPLE;
  protected readonly source = signal(V13_SAMPLE);
  protected readonly check = computed(() => checkV13(this.source()));
  protected readonly fixCount = computed(() => this.check().findings.filter(finding => finding.certainty === 'fix').length);
  protected readonly reviewCount = computed(() => this.check().findings.filter(finding => finding.certainty === 'review').length);
}
