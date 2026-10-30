import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommandPalette } from '@core/layout/command-palette/command-palette';
import { Footer } from '@core/layout/footer/footer';
import { Header } from '@core/layout/header/header';
import { AnnouncementBanner } from '@core/layout/announcement-banner';
import { InstallDialog } from '@core/layout/install-dialog/install-dialog';
import { ScrollTop } from '@core/layout/scroll-top';

@Component({
  selector: 'rh-root',
  imports: [RouterOutlet, AnnouncementBanner, Header, Footer, CommandPalette, InstallDialog, ScrollTop],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <rh-announcement-banner />
    <rh-header />
    <main id="main" tabindex="-1">
      <router-outlet />
    </main>
    <rh-footer />
    <rh-command-palette />
    <rh-install-dialog />
    <rh-scroll-top />
  `,
  styles: `
    :host { display: flex; flex-direction: column; min-height: 100vh; }
    main { flex: 1; outline: none; }
  `,
})
export class App {}
