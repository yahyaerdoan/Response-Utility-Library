import { Injectable } from '@angular/core';
import { PackageRegistry, PackageStats } from '@core/ports/package-registry';

const FLAT_CONTAINER = 'https://api.nuget.org/v3-flatcontainer';
const SEARCH = 'https://azuresearch-usnc.nuget.org/query';

interface FlatContainerIndex {
  readonly versions?: readonly string[];
}

interface SearchResponse {
  readonly data?: readonly { readonly id: string; readonly totalDownloads?: number }[];
}

@Injectable()
export class NuGetRegistry extends PackageRegistry {
  override async versions(packageId: string): Promise<readonly string[]> {
    const index = await this.getJson<FlatContainerIndex>(`${FLAT_CONTAINER}/${packageId.toLowerCase()}/index.json`);
    return index?.versions ?? [];
  }

  override async stats(packageId: string): Promise<PackageStats | null> {
    const query = new URLSearchParams({ q: `packageid:${packageId}`, prerelease: 'true', semVerLevel: '2.0.0' });
    const response = await this.getJson<SearchResponse>(`${SEARCH}?${query}`);
    const match = response?.data?.find(item => item.id.toLowerCase() === packageId.toLowerCase());
    return match ? { totalDownloads: match.totalDownloads ?? 0 } : null;
  }

  private async getJson<T>(url: string): Promise<T | null> {
    try {
      const response = await fetch(url);
      return response.ok ? ((await response.json()) as T) : null;
    } catch {
      return null;
    }
  }
}
