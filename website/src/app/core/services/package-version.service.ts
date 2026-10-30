import { computed, Injectable, inject, signal } from '@angular/core';
import { ASPNETCORE_PACKAGE, CORE_PACKAGE } from '@core/domain/install-commands';
import { PackageRegistry } from '@core/ports/package-registry';

interface Semver {
  readonly raw: string;
  readonly parts: readonly number[];
  readonly prerelease: readonly (string | number)[];
}

@Injectable({ providedIn: 'root' })
export class PackageVersionService {
  private readonly registry = inject(PackageRegistry);
  private readonly allVersions = signal<readonly string[]>([]);
  private readonly downloads = signal<number | null>(null);
  private loading: Promise<void> | null = null;

  readonly stable = computed(() => latest(this.allVersions().filter(version => !version.includes('-'))));
  readonly beta = computed(() => latest(this.allVersions().filter(version => version.includes('-'))));
  readonly totalDownloads = this.downloads.asReadonly();
  readonly releaseCount = computed(() => this.allVersions().filter(version => !version.includes('-')).length);

  load(): Promise<void> {
    this.loading ??= this.fetch();
    return this.loading;
  }

  private async fetch(): Promise<void> {
    const [versions, core, aspnet] = await Promise.all([
      this.registry.versions(CORE_PACKAGE),
      this.registry.stats(CORE_PACKAGE),
      this.registry.stats(ASPNETCORE_PACKAGE),
    ]);
    this.allVersions.set(versions);
    if (core || aspnet) {
      this.downloads.set((core?.totalDownloads ?? 0) + (aspnet?.totalDownloads ?? 0));
    }
  }
}

function latest(versions: readonly string[]): string | null {
  const sorted = versions.map(parse).sort(compare);
  return sorted.length > 0 ? sorted[sorted.length - 1].raw : null;
}

function parse(raw: string): Semver {
  const [core, pre] = raw.split('-', 2);
  return {
    raw,
    parts: core.split('.').map(Number),
    prerelease: pre ? pre.split('.').map(part => (/^\d+$/.test(part) ? Number(part) : part)) : [],
  };
}

function compare(a: Semver, b: Semver): number {
  for (let index = 0; index < 3; index++) {
    const diff = (a.parts[index] ?? 0) - (b.parts[index] ?? 0);
    if (diff !== 0) {
      return diff;
    }
  }
  if (a.prerelease.length === 0 || b.prerelease.length === 0) {
    return b.prerelease.length - a.prerelease.length;
  }
  for (let index = 0; index < Math.max(a.prerelease.length, b.prerelease.length); index++) {
    const left = a.prerelease[index];
    const right = b.prerelease[index];
    if (left === undefined || right === undefined) {
      return left === undefined ? -1 : 1;
    }
    if (left !== right) {
      return typeof left === 'number' && typeof right === 'number' ? left - right : String(left).localeCompare(String(right));
    }
  }
  return 0;
}
