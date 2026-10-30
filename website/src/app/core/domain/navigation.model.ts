export interface ToolLink {
  readonly label: string;
  readonly route: string;
  readonly icon: string;
  readonly description: string;
  readonly isNew?: boolean;
}

export interface NavLink {
  readonly label: string;
  readonly route: string;
  readonly icon: string;
  readonly exact?: boolean;
}
