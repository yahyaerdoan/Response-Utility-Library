export type HeroFeature = 'guard' | 'chain' | 'map' | 'sideEffect';

export type ApiStyle = 'Minimal API' | 'MVC controller';

export interface HeroFeatureOption {
  readonly id: HeroFeature;
  readonly label: string;
  readonly icon: string;
}

export interface HeroOutcome {
  readonly code: number;
  readonly label: string;
}

export const HERO_FEATURES: readonly HeroFeatureOption[] = [
  { id: 'guard', label: 'Guard', icon: 'bi-shield-check' },
  { id: 'chain', label: 'Chain', icon: 'bi-link-45deg' },
  { id: 'map', label: 'Map to DTO', icon: 'bi-arrow-left-right' },
  { id: 'sideEffect', label: 'Side effect', icon: 'bi-lightning-charge' },
];

export class HeroComposer {
  compose(features: ReadonlySet<HeroFeature>, style: ApiStyle): string {
    return `${this.endpoint(features, style)}\n\n${this.service(features)}`;
  }

  outcomes(features: ReadonlySet<HeroFeature>): readonly HeroOutcome[] {
    const outcomes: HeroOutcome[] = [
      { code: 200, label: features.has('map') ? 'ProductDto' : 'Product' },
      { code: 404, label: 'Not Found' },
    ];
    if (features.has('guard')) {
      outcomes.push({ code: 422, label: 'Validation Failed' });
    }
    if (features.has('chain')) {
      outcomes.push({ code: 409, label: 'Conflict from pricing' });
    }
    return outcomes;
  }

  private endpoint(features: ReadonlySet<HeroFeature>, style: ApiStyle): string {
    const type = this.dataType(features);
    if (style === 'MVC controller') {
      return [
        '[HttpGet("{id:int}")]',
        'public async Task<IActionResult> Get(int id)',
        '    => (await _products.GetAsync(id)).ToActionResult(HttpContext);',
      ].join('\n');
    }
    return [
      'app.MapGet("/api/products/{id:int}", async (int id, ProductService products, HttpContext http)',
      `    => (await products.GetAsync(id)).ToResult(http)); // IOperationResult<${type}>`,
    ].join('\n');
  }

  private service(features: ReadonlySet<HeroFeature>): string {
    const type = this.dataType(features);
    const lines = [`public Task<IOperationResult<${type}>> GetAsync(int id)`, '    => _products.GetByIdAsync(id)'];
    if (features.has('guard')) {
      lines.push('        .EnsureAsync(p => p.IsPublished, "Product is not published.")');
    }
    if (features.has('chain')) {
      lines.push('        .BindAsync(p => _pricing.ApplyDiscountAsync(p))');
    }
    if (features.has('map')) {
      lines.push('        .MapAsync(p => p.ToDto())');
    }
    if (features.has('sideEffect')) {
      const name = features.has('map') ? 'dto' : 'p';
      lines.push(`        .OnSuccessAsync(${name} => _cache.SetAsync(${name}))`);
    }
    lines[lines.length - 1] += ';';
    return lines.join('\n');
  }

  private dataType(features: ReadonlySet<HeroFeature>): string {
    return features.has('map') ? 'ProductDto' : 'Product';
  }
}
