import { UserToken } from './user-token';

class PaginationParams {
  currentPage: number = 1;
  pageSize: number = 5;
}

export class UserParams extends PaginationParams {
  gender: string = undefined;
  minAge: number = 18;
  maxAge: number = 99;
  orderBy = 'LastActive';

  constructor(user: UserToken) {
    super();
    this.gender = user.gender === 'male' ? 'female' : 'male';
  }
}

export class LikesParams extends PaginationParams {
  predicate = 'Liked';
}

export class MessageParams extends PaginationParams {
  container = 'Unread';
}
