import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class InstallDialogService {
  private readonly visible = signal(false);

  readonly isOpen = this.visible.asReadonly();

  open(): void {
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }
}
