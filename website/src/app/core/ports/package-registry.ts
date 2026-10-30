export interface PackageStats {
  readonly totalDownloads: number;
}

export abstract class PackageRegistry {
  abstract versions(packageId: string): Promise<readonly string[]>;
  abstract stats(packageId: string): Promise<PackageStats | null>;
}
