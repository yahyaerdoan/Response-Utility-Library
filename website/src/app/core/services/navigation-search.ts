import { Injectable } from '@angular/core';
import { SearchEntry } from '@core/domain/search.model';
import { PRIMARY_NAV, TOOL_LINKS } from '@core/navigation';
import { SEARCH_PROVIDERS, SearchProvider } from '@core/ports/search-provider';

const DESCRIPTIONS: Readonly<Record<string, string>> = {
  '/docs': 'Guides, concepts and API reference',
  '/statuses': 'All 62 statuses with factories and Problem Details',
  '/tools': 'Status wizard, overload picker, upgrade checker and more',
  '/recipes': 'Integrations with MediatR, FluentValidation, OpenAPI, Angular and more',
  '/gems': 'Lesser-known features worth knowing',
  '/changelog': 'What changed in each release',
};

@Injectable()
class NavigationSearchProvider implements SearchProvider {
  readonly name = 'navigation';

  entries(): Promise<readonly SearchEntry[]> {
    const navEntries: SearchEntry[] = PRIMARY_NAV.map(link => ({
      category: 'Tools',
      title: link.label,
      subtitle: DESCRIPTIONS[link.route] ?? '',
      icon: link.icon,
      route: link.route,
    }));
    const navRoutes = new Set(PRIMARY_NAV.map(link => link.route));
    const toolEntries: SearchEntry[] = TOOL_LINKS.filter(tool => !navRoutes.has(tool.route)).map(tool => ({
      category: 'Tools',
      title: tool.label,
      subtitle: tool.description,
      icon: tool.icon,
      route: tool.route,
    }));
    return Promise.resolve([...navEntries, ...toolEntries]);
  }
}

export const provideNavigationSearch = () => ({ provide: SEARCH_PROVIDERS, useClass: NavigationSearchProvider, multi: true });
