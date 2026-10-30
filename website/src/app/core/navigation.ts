import { NavLink, ToolLink } from '@core/domain/navigation.model';

export const PRIMARY_NAV: readonly NavLink[] = [
  { label: 'Docs', route: '/docs', icon: 'bi-journal-text' },
  { label: 'Tools', route: '/tools', icon: 'bi-tools' },
  { label: 'Status explorer', route: '/statuses', icon: 'bi-grid-3x3-gap' },
  { label: 'Recipes', route: '/recipes', icon: 'bi-journal-code' },
  { label: 'Hidden gems', route: '/gems', icon: 'bi-gem' },
  { label: 'Changelog', route: '/changelog', icon: 'bi-clock-history' },
];

export const TOOL_LINKS: readonly ToolLink[] = [
  { label: 'Status wizard', route: '/status-wizard', icon: 'bi-signpost-split', description: 'Answer a few yes/no questions and get the right status, the facade call and why.', isNew: true },
  { label: 'Overload picker', route: '/async-picker', icon: 'bi-shuffle', description: 'Pick a source and a next step; see which Map/Bind overload runs and what type comes out.', isNew: true },
  { label: 'v13 upgrade checker', route: '/migrate', icon: 'bi-arrow-up-circle', description: 'Paste code, see what breaks in v13, copy the fixed version.', isNew: true },
  { label: 'Status explorer', route: '/statuses', icon: 'bi-grid-3x3-gap', description: 'All 62 statuses with their factories, defaults and the Problem Details they produce.' },
  { label: 'Playground', route: '/playground', icon: 'bi-sliders', description: 'Build a result and see the C#, the HTTP response and a unit test.' },
  { label: 'Pipeline visualizer', route: '/pipeline', icon: 'bi-diagram-3', description: 'Watch a Map/Bind/Ensure chain run step by step and short-circuit on failure.' },
];

export const EXTERNAL_LINKS = {
  github: 'https://github.com/yahyaerdoan/Response-Utility-Library',
  nugetCore: 'https://www.nuget.org/packages/ResponseResultHandler',
  nugetAspNetCore: 'https://www.nuget.org/packages/ResponseResultHandler.AspNetCore',
} as const;

export const ANNOUNCEMENT = {
  id: 'v13.1',
  text: 'ResponseResultHandler 13.1: chain MediatR queries and commands in one expression.',
  cta: 'See what is new',
  link: '/changelog/13.1',
} as const;
