import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { ContentRenderer } from '@shared/ui/content-renderer';
import { FragmentLinkDirective } from '@shared/ui/fragment-link.directive';
import { ApiStylePivot } from '@shared/ui/api-style-pivot';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { flattenBlocks } from '@core/domain/content.model';
import { EXTERNAL_LINKS } from '@core/navigation';
import { DocsRepository } from '../application/docs-repository';
import { DocActions } from './doc-actions';
import { DocToc } from './doc-toc';

@Component({
  selector: 'rh-doc-page',
  imports: [RouterLink, ContentRenderer, DocToc, FragmentLinkDirective, ApiStylePivot, DocActions],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './doc-page.html',
  styleUrl: './doc-page.scss',
})
export class DocPageView {
  private readonly repository = inject(DocsRepository);
  private readonly titleService = inject(Title);

  readonly slug = input.required<string>();

  protected readonly page = computed(() => this.repository.find(this.slug()));
  protected readonly usesApiStyle = computed(() =>
    (this.page()?.sections ?? []).some(section => flattenBlocks(section.blocks).some(block => block.kind === 'tabs' && block.syncKey === TAB_GROUPS.apiStyle)),
  );
  protected readonly neighbours = computed(() => this.repository.neighbours(this.slug()));
  protected readonly group = computed(() => this.repository.groups().find(g => g.pages.some(p => p.slug === this.slug()))?.title ?? '');
  protected readonly editUrl = `${EXTERNAL_LINKS.github}/blob/master/README.md`;

  constructor() {
    effect(() => {
      const page = this.page();
      this.titleService.setTitle(`${page?.title ?? 'Not found'} · ResponseResultHandler`);
    });
  }
}
