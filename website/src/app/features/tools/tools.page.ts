import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TOOL_LINKS } from '@core/navigation';
import { PageHeader } from '@shared/ui/page-header';

@Component({
  selector: 'rh-tools-page',
  imports: [RouterLink, PageHeader],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-page-header eyebrow="Interactive" icon="bi-tools" title="Tools"
                    lead="Hands-on helpers that answer the questions people ask most, right in the browser." />

    <div class="container-xxl py-4 py-lg-5">
      <div class="row g-3">
        @for (tool of tools; track tool.route) {
          <div class="col-md-6 col-xl-4">
            <a class="tool surface surface-hover h-100" [routerLink]="tool.route">
              <span class="icon-tile"><i class="bi" [class]="tool.icon"></i></span>
              <span class="min-w-0">
                <span class="d-flex align-items-center gap-2">
                  <span class="fw-bold">{{ tool.label }}</span>
                  @if (tool.isNew) {
                    <span class="new">New</span>
                  }
                </span>
                <span class="d-block small text-muted-rh mt-1">{{ tool.description }}</span>
              </span>
            </a>
          </div>
        }
      </div>
    </div>
  `,
  styles: `
    .tool { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem; color: inherit; text-decoration: none; }
    .new { font-size: .65rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--rh-success); background: color-mix(in srgb, var(--rh-success) 15%, transparent); padding: .1rem .4rem; border-radius: .3rem; }
  `,
})
export class ToolsPage {
  protected readonly tools = TOOL_LINKS;
}
