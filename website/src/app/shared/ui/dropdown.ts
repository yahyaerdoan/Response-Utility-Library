import { ChangeDetectionStrategy, Component, ElementRef, inject, input, signal } from '@angular/core';

let nextId = 0;

@Component({
  selector: 'rh-dropdown',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'open.set(false)',
  },
  template: `
    <button type="button" class="trigger" [attr.aria-expanded]="open()" [attr.aria-controls]="menuId" aria-haspopup="menu"
            [attr.aria-label]="ariaLabel() ?? label()" (click)="open.set(!open())">
      <i class="bi" [class]="icon()"></i>
      <span class="label">{{ label() }}</span>
      <i class="bi bi-chevron-down caret"></i>
    </button>
    @if (open()) {
      <div class="menu" [class.end]="align() === 'end'" role="menu" [id]="menuId" (click)="onMenuClick($event)">
        <ng-content />
      </div>
    }
  `,
  styles: `
    :host { position: relative; display: inline-block; }
    .trigger { display: inline-flex; align-items: center; gap: .4rem; border: 1px solid var(--rh-border); border-radius: .55rem; background: var(--rh-surface); color: var(--rh-text); font-size: .82rem; font-weight: 600; padding: .35rem .65rem; }
    .trigger:hover, .trigger[aria-expanded="true"] { border-color: var(--rh-accent); }
    .caret { font-size: .65rem; color: var(--rh-muted); }
    .menu { position: absolute; top: calc(100% + .35rem); left: 0; z-index: 1050; min-width: 15rem; max-height: 70vh; overflow-y: auto; padding: .35rem; border: 1px solid var(--rh-border); border-radius: .75rem; background: var(--rh-surface); box-shadow: 0 1rem 2.5rem -1rem rgba(0, 0, 0, .45); animation: drop .12s ease-out; }
    .menu.end { left: auto; right: 0; }
    @keyframes drop { from { opacity: 0; transform: translateY(-4px); } }
    @media (max-width: 575.98px) { .label { display: none; } }
  `,
})
export class Dropdown {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly label = input.required<string>();
  readonly icon = input('bi-three-dots');
  readonly ariaLabel = input<string>();
  readonly align = input<'start' | 'end'>('start');

  protected readonly open = signal(false);
  protected readonly menuId = `rh-dropdown-${nextId++}`;

  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && event.target instanceof Node && !this.host.nativeElement.contains(event.target)) {
      this.open.set(false);
    }
  }

  protected onMenuClick(event: MouseEvent): void {
    if (event.target instanceof Element && event.target.closest('[role="menuitem"]')) {
      this.open.set(false);
    }
  }
}
