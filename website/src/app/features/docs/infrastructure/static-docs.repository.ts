import { Injectable } from '@angular/core';
import { DocsRepository } from '../application/docs-repository';
import { DOC_GROUPS } from '../data/docs.catalog';
import { DocGroup, DocNeighbours, DocPage } from '../domain/doc-page';

@Injectable()
export class StaticDocsRepository extends DocsRepository {
  private readonly ordered: readonly DocPage[] = DOC_GROUPS.flatMap(group => group.pages);

  override groups(): readonly DocGroup[] {
    return DOC_GROUPS;
  }

  override find(slug: string): DocPage | null {
    return this.ordered.find(page => page.slug === slug) ?? null;
  }

  override neighbours(slug: string): DocNeighbours {
    const index = this.ordered.findIndex(page => page.slug === slug);
    return {
      previous: index > 0 ? this.ordered[index - 1] : null,
      next: index >= 0 && index < this.ordered.length - 1 ? this.ordered[index + 1] : null,
    };
  }
}
