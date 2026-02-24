import {AxiosResponse} from 'axios';
import {API_URL, api} from './utils';
import {
  AccountDto,
  CreateAccountRequest,
  LoginRequest,
} from '@/models/auth.models';

const API_AUTH_URL = `${API_URL}/account`;

export function login(
  payload: LoginRequest,
): Promise<AxiosResponse<AccountDto>> {
  return api.post<AccountDto>(`${API_AUTH_URL}/login`, payload);
}

export function register(
  payload: CreateAccountRequest,
): Promise<AxiosResponse<void>> {
  return api.post<void>(`${API_AUTH_URL}/register`, payload);
}

export function getCurrentAccount(): Promise<AxiosResponse<AccountDto>> {
  return api.get<AccountDto>(`${API_AUTH_URL}/current`, {
    headers: {'requires-auth': true},
  });
}

export function forgotPassword(
  email: string,
  resetPasswordLink: string,
): Promise<AxiosResponse<void>> {
  return api.post<void>(`${API_AUTH_URL}/forgot-password`, {
    email,
    resetPasswordLink,
  });
}

export function resetPassword(
  email: string,
  newPassword: string,
): Promise<AxiosResponse<void>> {
  return api.post<void>(`${API_AUTH_URL}/reset-password`, {email, newPassword});
}
