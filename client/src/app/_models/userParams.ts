import {User} from "./user";
import {paginationParams} from "./paginationParams";

export class UserParams extends paginationParams {
  gender: string;
  minAge = 18;
  maxAge = 99;
  orderBy = 'lastActive';

  constructor(user: User) {
    super();
    this.gender = user.gender === 'male' ? 'female' : 'male';
  }
}
