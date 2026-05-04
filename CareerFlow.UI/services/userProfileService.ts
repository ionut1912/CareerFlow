import {CreateUserProfileRequest} from '@/models/userProfile.models';
import {api, API_URL} from './utils';
import {AxiosResponse} from 'axios';

const USER_PROFILE_URL = `${API_URL}/user-profile`;

export function saveUserProfile(
  userProfileRequest: CreateUserProfileRequest,
): Promise<AxiosResponse<string>> {
  return api.post<string>(USER_PROFILE_URL, userProfileRequest, {
    headers: {'requires-auth': ''},
  });
}
