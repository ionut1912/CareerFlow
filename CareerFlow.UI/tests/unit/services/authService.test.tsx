import {CreateAccountRequest, LoginRequest} from '@/models/auth.models';
import {
  forgotPassword,
  getCurrentAccount,
  login,
  register,
  resetPassword,
} from '@/services/authService';
import {api, API_URL} from '@/services/utils';

// 1. Mock the dependencies from './utils'
jest.mock('@/services/utils', () => ({
  API_URL: 'https://mock-api.example.com/api',
  api: {
    post: jest.fn(),
    get: jest.fn(),
  },
}));

describe('Auth API Service', () => {
  const API_AUTH_URL = `${API_URL}/account`;

  // Clear mock data before each test to prevent cross-test contamination
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should call api.post with the correct URL and payload', async () => {
      const mockPayload: LoginRequest = {
        email: 'test@test.com',
        password: 'password123',
      };
      const mockResponse = {data: {id: 1, email: 'test@test.com'}}; // Mocked AccountDto

      (api.post as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await login(mockPayload);

      expect(api.post).toHaveBeenCalledTimes(1);
      expect(api.post).toHaveBeenCalledWith(
        `${API_AUTH_URL}/login`,
        mockPayload,
      );
      expect(result).toEqual(mockResponse);
    });
  });

  describe('register', () => {
    it('should call api.post with the correct URL and payload', async () => {
      const mockPayload: CreateAccountRequest = {
        email: 'test@test.com',
        password: 'password123',
        name: 'John Doe',
      };

      (api.post as jest.Mock).mockResolvedValueOnce({status: 201});

      await register(mockPayload);

      expect(api.post).toHaveBeenCalledTimes(1);
      expect(api.post).toHaveBeenCalledWith(
        `${API_AUTH_URL}/register`,
        mockPayload,
      );
    });
  });

  describe('getCurrentAccount', () => {
    it('should call api.get with the correct URL and requires-auth header', async () => {
      const mockResponse = {data: {id: 1, email: 'test@test.com'}};

      (api.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await getCurrentAccount();

      expect(api.get).toHaveBeenCalledTimes(1);
      expect(api.get).toHaveBeenCalledWith(`${API_AUTH_URL}/current`, {
        headers: {'requires-auth': ''},
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('forgotPassword', () => {
    it('should call api.post with the correct URL and email payload', async () => {
      const testEmail = 'test@test.com';

      (api.post as jest.Mock).mockResolvedValueOnce({status: 200});

      await forgotPassword(testEmail);

      expect(api.post).toHaveBeenCalledTimes(1);
      expect(api.post).toHaveBeenCalledWith(`${API_AUTH_URL}/forgot-password`, {
        email: testEmail,
      });
    });
  });

  describe('resetPassword', () => {
    it('should call api.post with the correct URL, email, newPassword, and token', async () => {
      const email = 'test@test.com';
      const newPassword = 'newSecurePassword!';
      const token = 'abc123mockToken';

      (api.post as jest.Mock).mockResolvedValueOnce({status: 200});

      await resetPassword(email, newPassword, token);

      expect(api.post).toHaveBeenCalledTimes(1);
      expect(api.post).toHaveBeenCalledWith(`${API_AUTH_URL}/reset-password`, {
        email,
        newPassword,
        token,
      });
    });
  });
});
