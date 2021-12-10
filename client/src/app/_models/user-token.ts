export interface UserToken {
    userName: string;
    token: string;
    photoUrl: string;
    knownAs: string;
    gender: string;
    roles: string[];
}

export interface UserUpdateNoticationData {
    photoUrl: string;
    knownAs: string;
    gender: string;
}