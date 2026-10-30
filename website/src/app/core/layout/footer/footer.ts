import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EXTERNAL_LINKS, PRIMARY_NAV } from '@core/navigation';
import { Preferences } from './preferences';

@Component({
  selector: 'rh-footer',
  imports: [RouterLink, Preferences],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <footer class="site-footer">
      <div class="container-xxl py-5">
        <div class="row g-4">
          <div class="col-lg-5">
            <strong class="d-block mb-2">ResponseResultHandler</strong>
            <p class="text-muted-rh small mb-0">A Result-pattern library for .NET 8–10 with first-class ASP.NET Core and RFC 9457 Problem Details support. MIT licensed.</p>
          </div>
          <div class="col-12 col-sm-6 col-lg-3">
            <h2 class="foot-title">Explore</h2>
            <ul class="list-unstyled small">
              @for (link of nav; track link.route) { <li><a [routerLink]="link.route">{{ link.label }}</a></li> }
            </ul>
          </div>
          <div class="col-12 col-sm-6 col-lg-4">
            <h2 class="foot-title">Packages</h2>
            <ul class="list-unstyled small">
              <li><a [href]="links.nugetCore" target="_blank" rel="noopener">ResponseResultHandler <i class="bi bi-box-arrow-up-right"></i></a></li>
              <li><a [href]="links.nugetAspNetCore" target="_blank" rel="noopener">ResponseResultHandler.AspNetCore <i class="bi bi-box-arrow-up-right"></i></a></li>
              <li><a [href]="links.github" target="_blank" rel="noopener">Source on GitHub <i class="bi bi-box-arrow-up-right"></i></a></li>
            </ul>
          </div>
        </div>
        <div class="bottom">
          <p class="small text-muted-rh mb-0">© 2024–{{ year }} Yahya Erdogan · MIT License</p>
          <rh-preferences />
        </div>
      </div>
    </footer>
  `,
  styles: `
    .site-footer { border-top: 1px solid var(--rh-border); background: var(--rh-bg-soft); }
    .foot-title { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .1em; color: var(--rh-muted); }
    li { margin-bottom: .4rem; }
    a { color: var(--rh-text); overflow-wrap: anywhere; }
    a:hover { color: var(--rh-accent); }
    ul .bi { font-size: .7rem; }
    .bottom { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 1rem; margin-top: 2rem; padding-top: 1.5rem; border-top: 1px solid var(--rh-border); }
  `,
})
export class Footer {
  protected readonly nav = PRIMARY_NAV;
  protected readonly links = EXTERNAL_LINKS;
  protected readonly year = new Date().getFullYear();
}
