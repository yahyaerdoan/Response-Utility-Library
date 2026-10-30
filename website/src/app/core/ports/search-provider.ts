import { InjectionToken } from '@angular/core';
import { SearchEntry } from '@core/domain/search.model';

export interface SearchProvider {
  readonly name: string;
  entries(): Promise<readonly SearchEntry[]>;
}

export const SEARCH_PROVIDERS = new InjectionToken<readonly SearchProvider[]>('SEARCH_PROVIDERS');
