import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PackageVersionService } from '@core/services/package-version.service';
import { CopyButton } from '@shared/ui/copy-button';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { PageHeader } from '@shared/ui/page-header';
import { RECIPE_CATEGORIES, RecipeCategory, RECIPES } from './recipes.data';

const MAX_TAGS = 3;

@Component({
  selector: 'rh-recipes-page',
  imports: [FormsModule, RouterLink, DecimalPipe, PageHeader, CopyButton, InlineMarkupPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './recipes.page.html',
  styleUrl: './recipes.page.scss',
})
export class RecipesPage {
  protected readonly packages = inject(PackageVersionService);

  readonly q = input<string>();
  readonly category = input<string>();

  protected readonly categories = RECIPE_CATEGORIES;
  protected readonly maxTags = MAX_TAGS;
  protected readonly query = linkedSignal(() => this.q() ?? '');
  protected readonly selected = linkedSignal<RecipeCategory | null>(() => RECIPE_CATEGORIES.find(item => item.toLowerCase() === this.category()?.toLowerCase()) ?? null);

  protected readonly visible = computed(() => {
    const query = this.query().trim().toLowerCase();
    const category = this.selected();
    return RECIPES.filter(
      recipe =>
        (!category || recipe.category === category) &&
        (!query || `${recipe.title} ${recipe.text} ${recipe.tags.join(' ')} ${recipe.category}`.toLowerCase().includes(query)),
    );
  });

  protected readonly total = RECIPES.length;
  protected readonly tagCount = new Set(RECIPES.flatMap(recipe => recipe.tags)).size;

  constructor() {
    void this.packages.load();
  }

  protected count(category: RecipeCategory): number {
    return RECIPES.filter(recipe => recipe.category === category).length;
  }
}
