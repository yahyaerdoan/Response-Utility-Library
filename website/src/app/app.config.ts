import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding, withInMemoryScrolling, withViewTransitions } from '@angular/router';
import { Clipboard } from '@core/ports/clipboard';
import { PackageRegistry } from '@core/ports/package-registry';
import { NuGetRegistry } from '@core/infrastructure/nuget-registry';
import { BrowserClipboard } from '@core/services/browser-clipboard';
import { provideNavigationSearch } from '@core/services/navigation-search';
import { skipSamePageTransitions } from '@core/services/view-transitions';
import { provideDocsSearch } from '@features/docs/docs.search';
import { provideGemsSearch } from '@features/gems/gems.search';
import { provideRecipesSearch } from '@features/recipes/recipes.search';
import { provideStatusSearch } from '@features/status-explorer/status.search';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withViewTransitions({ onViewTransitionCreated: skipSamePageTransitions }),
      withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }),
    ),
    { provide: Clipboard, useClass: BrowserClipboard },
    { provide: PackageRegistry, useClass: NuGetRegistry },
    provideNavigationSearch(),
    provideDocsSearch(),
    provideStatusSearch(),
    provideGemsSearch(),
    provideRecipesSearch(),
  ],
};
