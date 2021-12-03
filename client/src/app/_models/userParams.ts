import { UserToken } from './user-token';

export class UserParams {
  currentPage: number = 1;
  pageSize: number = 5;
  gender: string = undefined;
  minAge: number = 18;
  maxAge: number = 99;
  orderBy = 'LastActive';

  constructor(user: UserToken) {
    this.gender = user.gender === 'male' ? 'female' : 'male';
  }
}
