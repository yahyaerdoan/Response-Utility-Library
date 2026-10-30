import { DocGroup } from '../domain/doc-page';
import { ASYNC, COMBINE, FUNCTIONAL } from './composition';
import { EQUALITY, FIELD_ERRORS, GENERIC_SHORT_CIRCUITING, SERIALIZATION } from './advanced';
import { MINIMAL_APIS, MVC_CONTROLLERS, PROBLEM_DETAILS } from './aspnetcore';
import { BUILDING_RESULTS, CORE_CONTRACT, RESULT_FACADE, RESULT_STATUS_DOC } from './fundamentals';
import { INSTALLATION, INTRODUCTION, QUICK_START, USE_ANYWHERE } from './getting-started';
import { BEST_PRACTICES, CASE_STUDY, CLIENT_CONSUMPTION, PITFALLS, TESTING, VERTICAL_SLICE } from './guides';
import { API_REFERENCE, COMPARISON, FAQ, MIGRATION_V12, MIGRATION_V13 } from './reference';

export const DOC_GROUPS: readonly DocGroup[] = [
  { title: 'Get started', pages: [INTRODUCTION, INSTALLATION, QUICK_START, USE_ANYWHERE] },
  { title: 'Fundamentals', pages: [CORE_CONTRACT, RESULT_STATUS_DOC, RESULT_FACADE, BUILDING_RESULTS] },
  { title: 'ASP.NET Core', pages: [MVC_CONTROLLERS, MINIMAL_APIS, PROBLEM_DETAILS] },
  { title: 'Composition', pages: [FUNCTIONAL, COMBINE, ASYNC] },
  { title: 'Advanced', pages: [GENERIC_SHORT_CIRCUITING, FIELD_ERRORS, SERIALIZATION, EQUALITY] },
  { title: 'Guides', pages: [VERTICAL_SLICE, CLIENT_CONSUMPTION, TESTING, BEST_PRACTICES, PITFALLS, CASE_STUDY] },
  { title: 'Reference', pages: [API_REFERENCE, COMPARISON, MIGRATION_V13, MIGRATION_V12, FAQ] },
];
