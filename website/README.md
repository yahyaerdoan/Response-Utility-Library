# ResponseResultHandler documentation site

Angular 20 (standalone, zoneless, signals) + Bootstrap 5 SPA that documents the library in `../src`.
UI-only: no backend, all content is typed data inside the app.

```bash
npm install
npm start          # http://localhost:4200
npm run build      # dist/website
```

## Architecture

```
src/app/
├─ core/                     app-wide, framework-level concerns
│  ├─ domain/                pure models (content blocks, search entries, nav links)
│  ├─ ports/                 abstractions: Clipboard, SearchProvider (+ SEARCH_PROVIDERS token)
│  ├─ services/              ThemeService, SearchService, BrowserClipboard, CommandPaletteService
│  └─ layout/                header, footer, Ctrl+K command palette
├─ shared/
│  ├─ kernel/result-status   the 62 ResultStatus values: shared domain knowledge used by several slices
│  ├─ ui/                    code block, code tabs, callout, content renderer, page header, pipes
│  └─ util/                  highlighter, inline markup
└─ features/                 vertical slices, each lazy-loaded and self-contained
   ├─ home/
   ├─ docs/                  domain → application (DocsRepository port) → infrastructure (static repo) → ui
   ├─ status-explorer/
   ├─ playground/            domain: ResultBuilder, CSharpGenerator, AssertionGenerator, ResponseAdapter strategies
   ├─ pipeline/              domain: PipelineStep subclasses (Source/Ensure/Map/Bind/OnSuccess/OnFailure), Combine
   ├─ gems/
   └─ changelog/
```

- **Vertical slices:** every feature owns its data, domain logic, UI and route; slices never import each other.
- **Dependency inversion:** UI depends on ports (`DocsRepository`, `Clipboard`, `SearchProvider`), implementations are provided in `app.config.ts` / route providers.
- **Open/closed:** search is a multi-provider token: a slice contributes results by registering a provider, without touching `SearchService`. Response adapters and pipeline steps are strategy classes.
- **DRY:** docs are data rendered by one `ContentRenderer`; status data lives once in `shared/kernel`.

## Adding a docs page

1. Create a `DocPage` in `features/docs/data/*.ts` using the builders (`section`, `p`, `cs`, `table`, `tip`…).
2. Add it to a group in `features/docs/data/docs.catalog.ts`. Sidebar, prev/next, TOC and search pick it up automatically.
