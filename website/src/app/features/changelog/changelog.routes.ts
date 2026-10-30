import { Routes } from '@angular/router';
import { ChangelogPage } from './changelog.page';
import { ReleasePage } from './release.page';

export const CHANGELOG_ROUTES: Routes = [
  { path: '', title: 'Changelog · ResponseResultHandler', component: ChangelogPage },
  { path: ':version', component: ReleasePage },
];
