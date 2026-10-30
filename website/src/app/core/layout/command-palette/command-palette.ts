import { ChangeDetectionStrategy, Component, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { SearchHit } from '@core/domain/search.model';
import { CommandPaletteService } from '@core/services/command-palette.service';
import { KeyboardService } from '@core/services/keyboard.service';
import { SearchService } from '@core/services/search.service';

@Component({
  selector: 'rh-command-palette',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:keydown)': 'onGlobalKey($event)' },
  templateUrl: './command-palette.html',
  styleUrl: './command-palette.scss',
})
export class CommandPalette {
  private readonly search = inject(SearchService);
  private readonly router = inject(Router);
  protected readonly palette = inject(CommandPaletteService);
  protected readonly keyboard = inject(KeyboardService);
  private readonly input = viewChild<ElementRef<HTMLInputElement>>('queryInput');

  protected readonly query = signal('');
  protected readonly hits = signal<readonly SearchHit[]>([]);
  protected readonly cursor = signal(0);
  private requestId = 0;

  constructor() {
    effect(() => {
      const input = this.input()?.nativeElement;
      if (this.palette.isOpen() && input) {
        input.focus();
      }
    });
    effect(() => {
      if (this.palette.isOpen()) {
        void this.runSearch(this.query());
      }
    });
  }

  protected onGlobalKey(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.palette.toggle();
      return;
    }
    if (event.key === '/' && !this.palette.isOpen() && !isTyping(event.target)) {
      event.preventDefault();
      this.palette.open();
    }
  }

  protected onKey(event: KeyboardEvent): void {
    const count = this.hits().length;
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.cursor.update(i => (count ? (i + 1) % count : 0));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.cursor.update(i => (count ? (i - 1 + count) % count : 0));
        break;
      case 'Enter': {
        const hit = this.hits()[this.cursor()];
        if (hit) {
          this.go(hit);
        }
        break;
      }
      case 'Escape':
        this.close();
        break;
    }
  }

  protected go(hit: SearchHit): void {
    this.close();
    void this.router.navigate([hit.route], { fragment: hit.fragment, queryParams: hit.queryParams });
  }

  protected close(): void {
    this.palette.close();
    this.query.set('');
  }

  private async runSearch(query: string): Promise<void> {
    const id = ++this.requestId;
    const hits = await this.search.search(query);
    if (id === this.requestId) {
      this.hits.set(hits);
      this.cursor.set(0);
    }
  }
}

function isTyping(target: EventTarget | null): boolean {
  return target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));
}
