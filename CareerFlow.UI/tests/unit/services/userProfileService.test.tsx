import {saveUserProfile} from '@/services/userProfileService';
import {api, API_URL} from '@/services/utils';
import {CreateUserProfileRequest} from '@/models/userProfile.models';

jest.mock('@/services/utils', () => ({
  API_URL: 'https://mock-api.example.com/api',
  api: {
    post: jest.fn(),
  },
}));

describe('User Profile API Service', () => {
  const USER_PROFILE_URL = `${API_URL}/user-profile`;

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('saveUserProfile', () => {
    it('should call api.post with the correct URL, payload, and auth header', async () => {
      const mockPayload: CreateUserProfileRequest = {
        learningType: 'Visual',
        userTypes: ['Student'],
        domain: 'Software Engineering',
      };

      const mockResponse = {data: 'Profile saved successfully'};
      (api.post as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await saveUserProfile(mockPayload);

      expect(api.post).toHaveBeenCalledTimes(1);
      expect(api.post).toHaveBeenCalledWith(USER_PROFILE_URL, mockPayload, {
        headers: {'requires-auth': ''},
      });
      expect(result).toEqual(mockResponse);
    });
  });
});
