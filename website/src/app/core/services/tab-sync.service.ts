import { computed, DOCUMENT, Injectable, inject, Signal, signal } from '@angular/core';
import { TAB_GROUPS } from '@core/domain/tab-groups';

interface UrlBinding {
  readonly param: string;
  readonly values: Readonly<Record<string, string>>;
}

const URL_BINDINGS: Readonly<Record<string, UrlBinding>> = {
  [TAB_GROUPS.apiStyle]: { param: 'style', values: { 'Minimal API': 'minimal', 'MVC controller': 'mvc' } },
};

const STORAGE_KEY = 'rh-tab-sync';

@Injectable({ providedIn: 'root' })
export class TabSyncService {
  private readonly document = inject(DOCUMENT);
  private readonly selections = signal<Readonly<Record<string, string>>>({ ...this.restore(), ...this.fromUrl() });

  selection(group: string): Signal<string | null> {
    return computed(() => this.selections()[group] ?? null);
  }

  select(group: string, label: string): void {
    this.selections.update(current => ({ ...current, [group]: label }));
    this.persist();
    this.writeUrl(group, label);
  }

  private fromUrl(): Record<string, string> {
    const view = this.document.defaultView;
    if (!view) {
      return {};
    }
    const params = new URLSearchParams(view.location.search);
    const result: Record<string, string> = {};
    for (const [group, binding] of Object.entries(URL_BINDINGS)) {
      const value = params.get(binding.param);
      const label = Object.entries(binding.values).find(([, slug]) => slug === value)?.[0];
      if (label) {
        result[group] = label;
      }
    }
    return result;
  }

  private writeUrl(group: string, label: string): void {
    const binding = URL_BINDINGS[group];
    const slug = binding?.values[label];
    const view = this.document.defaultView;
    if (!binding || !slug || !view) {
      return;
    }
    const url = new URL(view.location.href);
    url.searchParams.set(binding.param, slug);
    view.history.replaceState(view.history.state, '', `${url.pathname}${url.search}${url.hash}`);
  }

  private restore(): Record<string, string> {
    try {
      const raw = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
      const parsed: unknown = raw ? JSON.parse(raw) : {};
      return typeof parsed === 'object' && parsed !== null ? (parsed as Record<string, string>) : {};
    } catch {
      return {};
    }
  }

  private persist(): void {
    try {
      this.document.defaultView?.localStorage.setItem(STORAGE_KEY, JSON.stringify(this.selections()));
    } catch {
      return;
    }
  }
}
