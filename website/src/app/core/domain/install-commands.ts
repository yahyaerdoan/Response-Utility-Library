import { CodeSnippet } from './content.model';

export type ReleaseChannel = 'stable' | 'beta';

export interface InstallOptions {
  readonly channel: ReleaseChannel;
  readonly includeAspNetCore: boolean;
  readonly version: string | null;
}

export const CORE_PACKAGE = 'ResponseResultHandler';
export const ASPNETCORE_PACKAGE = 'ResponseResultHandler.AspNetCore';

export function installSnippets(options: InstallOptions): readonly CodeSnippet[] {
  const packages = options.includeAspNetCore ? [CORE_PACKAGE, ASPNETCORE_PACKAGE] : [CORE_PACKAGE];
  const beta = options.channel === 'beta';
  const floating = beta ? '12.*-*' : '12.*';
  const pinned = options.version ?? floating;

  return [
    {
      label: '.NET CLI',
      language: 'bash',
      code: packages.map(id => `dotnet add package ${id}${beta ? ' --prerelease' : ''}`).join('\n'),
    },
    {
      label: 'PackageReference',
      language: 'xml',
      code: ['<ItemGroup>', ...packages.map(id => `  <PackageReference Include="${id}" Version="${floating}" />`), '</ItemGroup>'].join('\n'),
    },
    {
      label: 'Central Package Management',
      language: 'xml',
      code: [
        '<!-- Directory.Packages.props -->',
        ...(options.version ? [] : ['<PropertyGroup>', '  <CentralPackageFloatingVersionsEnabled>true</CentralPackageFloatingVersionsEnabled>', '</PropertyGroup>']),
        '<ItemGroup>',
        ...packages.map(id => `  <PackageVersion Include="${id}" Version="${pinned}" />`),
        '</ItemGroup>',
      ].join('\n'),
    },
    {
      label: 'Package Manager',
      language: 'powershell',
      code: packages.map(id => `Install-Package ${id}${beta ? ' -Prerelease' : ''}`).join('\n'),
    },
  ];
}
