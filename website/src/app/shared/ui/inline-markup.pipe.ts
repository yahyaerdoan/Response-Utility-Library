import { Pipe, PipeTransform } from '@angular/core';
import { renderInlineMarkup } from '@shared/util/inline-markup';

@Pipe({ name: 'inlineMarkup' })
export class InlineMarkupPipe implements PipeTransform {
  transform(value: string): string {
    return renderInlineMarkup(value);
  }
}
