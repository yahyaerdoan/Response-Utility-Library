import { Injectable } from '@angular/core';
import { SearchEntry } from '@core/domain/search.model';
import { SEARCH_PROVIDERS, SearchProvider } from '@core/ports/search-provider';

@Injectable()
class RecipesSearchProvider implements SearchProvider {
  readonly name = 'recipes';

  async entries(): Promise<readonly SearchEntry[]> {
    const { RECIPES } = await import('./recipes.data');
    return RECIPES.map(recipe => ({
      category: 'Recipes',
      title: recipe.title,
      subtitle: recipe.text.replaceAll('`', ''),
      icon: recipe.icon,
      route: recipe.route,
      fragment: recipe.fragment,
      keywords: `${recipe.category} ${recipe.tags.join(' ')}`,
    }));
  }
}

export const provideRecipesSearch = () => ({ provide: SEARCH_PROVIDERS, useClass: RecipesSearchProvider, multi: true });
