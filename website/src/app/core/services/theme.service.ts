import { computed, DestroyRef, DOCUMENT, effect, Injectable, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';
export type ThemePreference = ThemeMode | 'auto';

const STORAGE_KEY = 'rh-theme';
const DARK_QUERY = '(prefers-color-scheme: dark)';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly systemDark = signal(this.matchesDark());
  private readonly stored = signal<ThemePreference>(this.restore());

  readonly preference = this.stored.asReadonly();
  readonly current = computed<ThemeMode>(() => {
    const preference = this.stored();
    if (preference === 'auto') {
      return this.systemDark() ? 'dark' : 'light';
    }
    return preference;
  });

  constructor() {
    const media = this.document.defaultView?.matchMedia?.(DARK_QUERY);
    const listener = (event: MediaQueryListEvent) => this.systemDark.set(event.matches);
    media?.addEventListener('change', listener);
    inject(DestroyRef).onDestroy(() => media?.removeEventListener('change', listener));

    effect(() => this.document.documentElement.setAttribute('data-bs-theme', this.current()));
    effect(() => this.persist(this.stored()));
  }

  toggle(): void {
    this.stored.set(this.current() === 'dark' ? 'light' : 'dark');
  }

  setPreference(preference: ThemePreference): void {
    this.stored.set(preference);
  }

  private matchesDark(): boolean {
    return this.document.defaultView?.matchMedia?.(DARK_QUERY).matches ?? true;
  }

  private restore(): ThemePreference {
    try {
      const value = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
      return value === 'light' || value === 'dark' || value === 'auto' ? value : 'auto';
    } catch {
      return 'auto';
    }
  }

  private persist(preference: ThemePreference): void {
    try {
      this.document.defaultView?.localStorage.setItem(STORAGE_KEY, preference);
    } catch {
      return;
    }
  }
}
