import { Injectable } from '@angular/core';
import { flattenBlocks } from '@core/domain/content.model';
import { SearchEntry } from '@core/domain/search.model';
import { SEARCH_PROVIDERS, SearchProvider } from '@core/ports/search-provider';

@Injectable()
class DocsSearchProvider implements SearchProvider {
  readonly name = 'docs';

  async entries(): Promise<readonly SearchEntry[]> {
    const { DOC_GROUPS } = await import('./data/docs.catalog');
    return DOC_GROUPS.flatMap(group =>
      group.pages.flatMap(page => [
        { category: 'Docs', title: page.title, subtitle: `${group.title} · ${page.summary}`, icon: page.icon, route: `/docs/${page.slug}` } satisfies SearchEntry,
        ...page.sections.map(
          section =>
            ({
              category: 'Docs',
              title: section.title,
              subtitle: `${page.title}`,
              icon: 'bi-hash',
              route: `/docs/${page.slug}`,
              fragment: section.id,
              keywords: flattenBlocks(section.blocks).map(block => ('text' in block ? block.text : '')).join(' '),
            }) satisfies SearchEntry,
        ),
      ]),
    );
  }
}

export const provideDocsSearch = () => ({ provide: SEARCH_PROVIDERS, useClass: DocsSearchProvider, multi: true });
