export type SearchCategory = 'Docs' | 'Status' | 'Tools' | 'Gems' | 'Recipes';

export interface SearchEntry {
  readonly category: SearchCategory;
  readonly title: string;
  readonly subtitle: string;
  readonly icon: string;
  readonly route: string;
  readonly fragment?: string;
  readonly queryParams?: Readonly<Record<string, string>>;
  readonly keywords?: string;
}

export interface SearchHit extends SearchEntry {
  readonly score: number;
}
