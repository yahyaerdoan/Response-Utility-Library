import { DOCUMENT, Injectable, inject } from '@angular/core';
import { Clipboard } from '@core/ports/clipboard';

@Injectable()
export class BrowserClipboard extends Clipboard {
  private readonly document = inject(DOCUMENT);

  override async copy(text: string): Promise<boolean> {
    const clipboard = this.document.defaultView?.navigator.clipboard;
    if (!clipboard) {
      return false;
    }
    try {
      await clipboard.writeText(text);
      return true;
    } catch {
      return false;
    }
  }
}
