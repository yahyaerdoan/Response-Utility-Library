import { Injectable } from '@angular/core';
import { SearchEntry } from '@core/domain/search.model';
import { SEARCH_PROVIDERS, SearchProvider } from '@core/ports/search-provider';
import { RESULT_STATUSES } from '@shared/kernel/result-status';

@Injectable()
class StatusSearchProvider implements SearchProvider {
  readonly name = 'statuses';

  entries(): Promise<readonly SearchEntry[]> {
    return Promise.resolve(
      RESULT_STATUSES.map(status => ({
        category: 'Status',
        title: `${status.code} ${status.name}`,
        subtitle: status.summary.replaceAll('`', ''),
        icon: 'bi-grid-3x3-gap',
        route: '/statuses',
        queryParams: { s: status.name },
        keywords: `Result.${status.name} ${status.defaultTitle}`,
      })),
    );
  }
}

export const provideStatusSearch = () => ({ provide: SEARCH_PROVIDERS, useClass: StatusSearchProvider, multi: true });
