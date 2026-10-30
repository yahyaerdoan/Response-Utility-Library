import { DocGroup, DocNeighbours, DocPage } from '../domain/doc-page';

export abstract class DocsRepository {
  abstract groups(): readonly DocGroup[];
  abstract find(slug: string): DocPage | null;
  abstract neighbours(slug: string): DocNeighbours;
}
