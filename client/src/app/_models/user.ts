import { Photo } from './photo';

export interface User {
  id: number;
  userName: string;
  photoUrl: string;
  age: number;
  knownAs: string;
  created: Date;
  lastActive: Date;
  gender: string;
  introduction: string;
  lookingFor: string;
  interests: string;
  city: string;
  country: string;
  isOnline: boolean;
  photos: Photo[];
}

export interface PresenceTrackerData {
  userName: string;
  isOnline: boolean;
}
