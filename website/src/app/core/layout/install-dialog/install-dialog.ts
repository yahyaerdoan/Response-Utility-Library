import { ChangeDetectionStrategy, Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { installSnippets, ReleaseChannel } from '@core/domain/install-commands';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { EXTERNAL_LINKS } from '@core/navigation';
import { InstallDialogService } from '@core/services/install-dialog.service';
import { PackageVersionService } from '@core/services/package-version.service';
import { CodeTabs } from '@shared/ui/code-tabs';

interface ChannelOption {
  readonly id: ReleaseChannel;
  readonly label: string;
  readonly description: string;
}

const CHANNELS: readonly ChannelOption[] = [
  { id: 'stable', label: 'Stable', description: 'Latest stable release for production use.' },
  { id: 'beta', label: 'Beta', description: 'Previews built from the beta branch (-beta.N). May change before release.' },
];

@Component({
  selector: 'rh-install-dialog',
  imports: [CodeTabs],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './install-dialog.html',
  styleUrl: './install-dialog.scss',
})
export class InstallDialog {
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private readonly versions = inject(PackageVersionService);
  protected readonly service = inject(InstallDialogService);

  protected readonly channels = CHANNELS;
  protected readonly installGroup = TAB_GROUPS.install;
  protected readonly links = EXTERNAL_LINKS;
  protected readonly channel = signal<ReleaseChannel>('stable');
  protected readonly includeAspNetCore = signal(true);
  protected readonly version = computed(() => (this.channel() === 'stable' ? this.versions.stable() : this.versions.beta()));
  protected readonly snippets = computed(() =>
    installSnippets({ channel: this.channel(), includeAspNetCore: this.includeAspNetCore(), version: this.version() }),
  );

  constructor() {
    effect(() => {
      const element = this.dialog().nativeElement;
      if (this.service.isOpen() && !element.open) {
        element.showModal();
        void this.versions.load();
      } else if (!this.service.isOpen() && element.open) {
        element.close();
      }
    });
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement) {
      this.service.close();
    }
  }
}
