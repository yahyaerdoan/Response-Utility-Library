import { Routes } from '@angular/router';
import { DocsRepository } from './application/docs-repository';
import { StaticDocsRepository } from './infrastructure/static-docs.repository';
import { DocPageView } from './ui/doc-page';
import { DocsLayout } from './ui/docs-layout';

export const DOCS_ROUTES: Routes = [
  {
    path: '',
    component: DocsLayout,
    providers: [{ provide: DocsRepository, useClass: StaticDocsRepository }],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'introduction' },
      { path: ':slug', component: DocPageView },
    ],
  },
];
