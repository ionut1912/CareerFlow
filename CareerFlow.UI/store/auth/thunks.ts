import {CreateAccountRequest, LoginRequest} from '@/models/auth.models';
import {
  login,
  loginWithGoogle,
  loginWithLinkedin,
  register,
} from '@/services/authService';
import {createAsyncThunk} from '@reduxjs/toolkit';
import {isAxiosError} from 'axios';

export const loginThunk = createAsyncThunk(
  'auth/login',
  async (payload: LoginRequest, {rejectWithValue}) => {
    try {
      const res = await login(payload);
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

export const loginWithGoogleThunk = createAsyncThunk(
  'auth/loginWithGoogle',
  async (idToken: string, {rejectWithValue}) => {
    try {
      const res = await loginWithGoogle(idToken);
      return res.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || 'Google login failed',
        );
      }
      return rejectWithValue('Google login failed');
    }
  },
);

export const loginWithLinkedinThunk = createAsyncThunk(
  'auth/loginWithLinkedin',
  async (code: string, {rejectWithValue}) => {
    try {
      const res = await loginWithLinkedin(code);
      return res.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || 'LinkedIn login failed',
        );
      }
      return rejectWithValue('LinkedIn login failed');
    }
  },
);
