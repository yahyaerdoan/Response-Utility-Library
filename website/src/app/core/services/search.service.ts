import { Injectable, inject } from '@angular/core';
import { SearchEntry, SearchHit } from '@core/domain/search.model';
import { SEARCH_PROVIDERS } from '@core/ports/search-provider';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly providers = inject(SEARCH_PROVIDERS);
  private index: Promise<readonly SearchEntry[]> | null = null;

  async search(query: string, limit = 12): Promise<readonly SearchHit[]> {
    const entries = await this.loadIndex();
    const terms = query.trim().toLowerCase().split(/\s+/).filter(Boolean);
    if (terms.length === 0) {
      return entries.slice(0, limit).map(entry => ({ ...entry, score: 0 }));
    }
    return entries
      .map(entry => ({ ...entry, score: this.score(entry, terms) }))
      .filter(hit => hit.score > 0)
      .sort((a, b) => b.score - a.score)
      .slice(0, limit);
  }

  private loadIndex(): Promise<readonly SearchEntry[]> {
    this.index ??= Promise.all(this.providers.map(provider => provider.entries())).then(groups => groups.flat());
    return this.index;
  }

  private score(entry: SearchEntry, terms: readonly string[]): number {
    const title = entry.title.toLowerCase();
    const body = `${entry.subtitle} ${entry.keywords ?? ''}`.toLowerCase();
    let total = 0;
    for (const term of terms) {
      const weight = title.startsWith(term) ? 6 : title.includes(term) ? 4 : body.includes(term) ? 1 : 0;
      if (weight === 0) {
        return 0;
      }
      total += weight;
    }
    return total;
  }
}
