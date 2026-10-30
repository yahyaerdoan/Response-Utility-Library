import { Routes } from '@angular/router';

const SUFFIX = ' · ResponseResultHandler';

export const routes: Routes = [
  { path: '', title: 'ResponseResultHandler: Results, not exceptions', loadComponent: () => import('@features/home/home.page').then(m => m.HomePage) },
  { path: 'docs', loadChildren: () => import('@features/docs/docs.routes').then(m => m.DOCS_ROUTES) },
  { path: 'statuses', title: `Status explorer${SUFFIX}`, loadComponent: () => import('@features/status-explorer/status-explorer.page').then(m => m.StatusExplorerPage) },
  { path: 'playground', title: `Playground${SUFFIX}`, loadComponent: () => import('@features/playground/ui/playground.page').then(m => m.PlaygroundPage) },
  { path: 'pipeline', title: `Pipeline visualizer${SUFFIX}`, loadComponent: () => import('@features/pipeline/ui/pipeline.page').then(m => m.PipelinePage) },
  { path: 'tools', title: `Tools${SUFFIX}`, loadComponent: () => import('@features/tools/tools.page').then(m => m.ToolsPage) },
  { path: 'status-wizard', title: `Status wizard${SUFFIX}`, loadComponent: () => import('@features/tools/status-wizard/status-wizard.page').then(m => m.StatusWizardPage) },
  { path: 'async-picker', title: `Overload picker${SUFFIX}`, loadComponent: () => import('@features/tools/async-picker/async-picker.page').then(m => m.AsyncPickerPage) },
  { path: 'migrate', title: `v13 upgrade checker${SUFFIX}`, loadComponent: () => import('@features/tools/migrate/migrate.page').then(m => m.MigratePage) },
  { path: 'recipes', title: `Recipes${SUFFIX}`, loadComponent: () => import('@features/recipes/recipes.page').then(m => m.RecipesPage) },
  { path: 'gems', title: `Hidden gems${SUFFIX}`, loadComponent: () => import('@features/gems/gems.page').then(m => m.GemsPage) },
  { path: 'changelog', loadChildren: () => import('@features/changelog/changelog.routes').then(m => m.CHANGELOG_ROUTES) },
  { path: '**', title: `Not found${SUFFIX}`, loadComponent: () => import('@features/not-found/not-found.page').then(m => m.NotFoundPage) },
];
