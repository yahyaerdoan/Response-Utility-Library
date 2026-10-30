import { computed, DOCUMENT, effect, Injectable, inject, signal } from '@angular/core';

export type KeyboardStyle = 'auto' | 'mac' | 'windows';

const STORAGE_KEY = 'rh-keyboard';

interface NavigatorWithUaData extends Navigator {
  readonly userAgentData?: { readonly platform?: string };
}

@Injectable({ providedIn: 'root' })
export class KeyboardService {
  private readonly document = inject(DOCUMENT);
  private readonly stored = signal<KeyboardStyle>(this.restore());

  readonly style = this.stored.asReadonly();
  readonly isMac = computed(() => {
    const style = this.stored();
    return style === 'auto' ? this.detectMac() : style === 'mac';
  });
  readonly modifier = computed(() => (this.isMac() ? '⌘' : 'Ctrl'));

  constructor() {
    effect(() => {
      try {
        this.document.defaultView?.localStorage.setItem(STORAGE_KEY, this.stored());
      } catch {
        return;
      }
    });
  }

  setStyle(style: KeyboardStyle): void {
    this.stored.set(style);
  }

  private detectMac(): boolean {
    const navigator = this.document.defaultView?.navigator as NavigatorWithUaData | undefined;
    const platform = navigator?.userAgentData?.platform ?? navigator?.platform ?? '';
    return /mac|iphone|ipad|ipod/i.test(platform);
  }

  private restore(): KeyboardStyle {
    try {
      const value = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
      return value === 'mac' || value === 'windows' ? value : 'auto';
    } catch {
      return 'auto';
    }
  }
}
