import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { CommandPaletteService } from '@core/services/command-palette.service';
import { InstallDialogService } from '@core/services/install-dialog.service';
import { KeyboardService } from '@core/services/keyboard.service';
import { PackageVersionService } from '@core/services/package-version.service';
import { CodeBlock } from '@shared/ui/code-block';
import { CodeTabs } from '@shared/ui/code-tabs';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { FlowStepper } from './ui/flow-stepper';
import { HeroComposerView } from './ui/hero-composer';
import { RefactorToggle } from './ui/refactor-toggle';
import { TerminalStory } from './ui/terminal-story';
import { AFTER, BEFORE, ECOSYSTEM, FEATURES, FLOW, INSTALL_SNIPPETS, SHOWCASE, STATS } from './home.content';

@Component({
  selector: 'rh-home-page',
  imports: [RouterLink, DecimalPipe, CodeBlock, CodeTabs, InlineMarkupPipe, HeroComposerView, FlowStepper, TerminalStory, RefactorToggle],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePage {
  protected readonly palette = inject(CommandPaletteService);
  protected readonly install = inject(InstallDialogService);
  protected readonly keyboard = inject(KeyboardService);
  protected readonly installSnippets = INSTALL_SNIPPETS;
  protected readonly installGroup = TAB_GROUPS.install;
  protected readonly stats = STATS;
  protected readonly features = FEATURES;
  protected readonly flow = FLOW;
  protected readonly before = BEFORE;
  protected readonly after = AFTER;
  protected readonly showcase = SHOWCASE;
  protected readonly ecosystem = ECOSYSTEM;
  protected readonly packages = inject(PackageVersionService);

  constructor() {
    void this.packages.load();
  }
}
