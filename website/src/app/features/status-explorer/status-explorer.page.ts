import { ChangeDetectionStrategy, Component, computed, ElementRef, inject, input, linkedSignal, signal, viewChild } from '@angular/core';
import { Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { findStatus, RESULT_STATUSES, ResultStatusInfo, STATUS_CLASS_LABELS, StatusClass } from '@shared/kernel/result-status';
import { CodeBlock } from '@shared/ui/code-block';
import { InlineMarkupPipe } from '@shared/ui/inline-markup.pipe';
import { PageHeader } from '@shared/ui/page-header';
import { responseBehaviour, sampleProblem, signaturesOf } from './status-signatures';

type ClassFilter = StatusClass | 'all';

@Component({
  selector: 'rh-status-explorer-page',
  imports: [FormsModule, RouterLink, PageHeader, CodeBlock, InlineMarkupPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './status-explorer.page.html',
  styleUrl: './status-explorer.page.scss',
})
export class StatusExplorerPage {
  private readonly location = inject(Location);
  private readonly detail = viewChild<ElementRef<HTMLElement>>('detail');

  readonly s = input<string>();

  protected readonly sheetOpen = signal(false);

  protected readonly filters: readonly { value: ClassFilter; label: string }[] = [
    { value: 'all', label: `All ${RESULT_STATUSES.length}` },
    ...([1, 2, 3, 4, 5] as StatusClass[]).map(value => ({ value, label: `${value}xx ${STATUS_CLASS_LABELS[value]}` })),
  ];

  protected readonly classFilter = signal<ClassFilter>('all');
  protected readonly query = signal('');

  protected readonly visible = computed(() => {
    const filter = this.classFilter();
    const query = this.query().trim().toLowerCase();
    return RESULT_STATUSES.filter(
      status =>
        (filter === 'all' || status.statusClass === filter) &&
        (!query || `${status.code} ${status.name} ${status.summary}`.toLowerCase().includes(query)),
    );
  });

  protected readonly selected = linkedSignal<ResultStatusInfo>(() => findStatus(this.s() ?? '') ?? findStatus('NotFound') ?? RESULT_STATUSES[0]);
  protected readonly signatures = computed(() => signaturesOf(this.selected()));
  protected readonly behaviour = computed(() => responseBehaviour(this.selected()));
  protected readonly problem = computed(() => sampleProblem(this.selected()));

  protected select(status: ResultStatusInfo): void {
    this.selected.set(status);
    this.location.replaceState(this.location.path().split('?')[0], `s=${encodeURIComponent(status.name)}`);
    this.detail()?.nativeElement.scrollTo({ top: 0 });
    this.sheetOpen.set(true);
  }
}
