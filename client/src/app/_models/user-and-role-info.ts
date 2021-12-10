export interface UserAndRolesInfo {
  id: number;
  userName: string;
  knownAs: string;
  roles: string[];
}

export interface RoleCheckboxData {
  checked: boolean;
  name: string;
}

export interface PhotoForAdminFeatureDto {
  id: number;
  url: string;
  userId: number;
  userName: string;
  knownAs: string;
}
