import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { EXTERNAL_LINKS, PRIMARY_NAV } from '@core/navigation';
import { CommandPaletteService } from '@core/services/command-palette.service';
import { InstallDialogService } from '@core/services/install-dialog.service';
import { KeyboardService } from '@core/services/keyboard.service';
import { ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'rh-header',
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './header.html',
  styleUrl: './header.scss',
  host: { '[class.scrolled]': 'scrolled()', '(window:scroll)': 'onScroll()' },
})
export class Header {
  protected readonly theme = inject(ThemeService);
  protected readonly palette = inject(CommandPaletteService);
  protected readonly install = inject(InstallDialogService);
  protected readonly keyboard = inject(KeyboardService);
  protected readonly nav = PRIMARY_NAV;
  protected readonly github = EXTERNAL_LINKS.github;
  protected readonly menuOpen = signal(false);
  protected readonly scrolled = signal(false);

  protected onScroll(): void {
    this.scrolled.set(window.scrollY > 8);
  }
}
