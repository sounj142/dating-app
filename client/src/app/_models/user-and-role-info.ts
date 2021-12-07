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