import { ContentSection } from '@core/domain/content.model';

export type BadgeTone = 'new' | 'accent' | 'muted';

export interface DocBadge {
  readonly label: string;
  readonly tone: BadgeTone;
}

export interface DocPage {
  readonly slug: string;
  readonly title: string;
  readonly icon: string;
  readonly summary: string;
  readonly sections: readonly ContentSection[];
  readonly badge?: DocBadge;
}

export interface DocGroup {
  readonly title: string;
  readonly pages: readonly DocPage[];
}

export interface DocNeighbours {
  readonly previous: DocPage | null;
  readonly next: DocPage | null;
}
