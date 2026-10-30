import { Injectable } from '@angular/core';
import { SearchEntry } from '@core/domain/search.model';
import { SEARCH_PROVIDERS, SearchProvider } from '@core/ports/search-provider';

@Injectable()
class GemsSearchProvider implements SearchProvider {
  readonly name = 'gems';

  async entries(): Promise<readonly SearchEntry[]> {
    const { GEMS } = await import('./gems.data');
    return GEMS.map(gem => ({
      category: 'Gems',
      title: gem.title.replaceAll('`', ''),
      subtitle: gem.text.replaceAll('`', '').replaceAll('**', ''),
      icon: gem.icon,
      route: '/gems',
      keywords: gem.area,
    }));
  }
}

export const provideGemsSearch = () => ({ provide: SEARCH_PROVIDERS, useClass: GemsSearchProvider, multi: true });
