import {CreateAccountRequest, LoginRequest} from '@/models/auth.models';
import {
  forgotPassword,
  getCurrentAccount,
  login,
  register,
  resetPassword,
} from '@/services/authService';
import {API_URL} from '@/services/utils';
import {secureStorage} from '@/utils/secureStorage';
import {createAsyncThunk} from '@reduxjs/toolkit';
import {isAxiosError} from 'axios';

export const loginThunk = createAsyncThunk(
  'auth/login',
  async (payload: LoginRequest, {rejectWithValue}) => {
    try {
      const res = await login(payload);
      const {token, refreshToken} = res.data;
      if (token && refreshToken) {
        await secureStorage.saveTokens(token, refreshToken);
      }
      return res.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(error.response?.data?.message || 'Login failed');
      }
      return rejectWithValue('Login failed');
    }
  },
);

export const registerThunk = createAsyncThunk(
  'auth/register',
  async (payload: CreateAccountRequest, {rejectWithValue}) => {
    try {
      await register(payload);
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || 'Registration failed',
        );
      }
      return rejectWithValue('Registration failed');
    }
  },
);

export const loginWithSocialThunk = createAsyncThunk(
  'auth/loginWithSocial',
  async (
    {token, refreshToken}: {token: string; refreshToken: string},
    {rejectWithValue},
  ) => {
    try {
      await secureStorage.saveTokens(token, refreshToken);
      const res = await getCurrentAccount();
      return {...res.data, token, refreshToken};
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || 'Social login failed',
        );
      }
      return rejectWithValue('Social login failed');
    }
  },
);

export const restoreSessionThunk = createAsyncThunk(
  'auth/restoreSession',
  async (_, {rejectWithValue}) => {
    try {
      const token = await secureStorage.getToken();
      const refreshToken = await secureStorage.getRefreshToken();
      if (!token || !refreshToken) return rejectWithValue('No session');
      const res = await getCurrentAccount();
      return {...res.data, token, refreshToken};
    } catch {
      await secureStorage.clearTokens();
      return rejectWithValue('Session expired');
    }
  },
);
export const requestPasswordResetThunk = createAsyncThunk(
  'auth/requestPasswordReset',
  async (payload: {email: string}, {rejectWithValue}) => {
    try {
      const resetPasswordLink = `${API_URL}/reset-password`;

      const response = await forgotPassword(payload.email, resetPasswordLink);

      return response.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message ||
            error.message ||
            'Eroare de conexiune la server',
        );
      }
    }
  },
);

export const resetPasswordThunk = createAsyncThunk(
  'auth/resetPassword',
  async (payload: {email: string; newPassword: string}, {rejectWithValue}) => {
    try {
      const response = await resetPassword(payload.email, payload.newPassword);

      return response.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message ||
            error.message ||
            'Eroare de conexiune la server',
        );
      }
      return rejectWithValue('Eroare de conexiune la server');
    }
  },
);

export const logoutThunk = createAsyncThunk('auth/logoutThunk', async () => {
  await secureStorage.clearTokens();
});
