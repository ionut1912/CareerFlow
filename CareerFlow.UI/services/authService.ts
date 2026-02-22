import {
  AccountDto,
  CreateAccountRequest,
  LoginRequest,
} from '@/models/auth.models';
import {api, API_URL} from './utils';
import {AxiosResponse} from 'axios';

export function register(
  request: CreateAccountRequest,
): Promise<AxiosResponse<string>> {
  return api.post<string>(`${API_URL}/account/register`, request);
}

export function login(
  request: LoginRequest,
): Promise<AxiosResponse<AccountDto>> {
  return api.post<AccountDto>(`${API_URL}/account/login`, request);
}

export function loginWithGoogle(
  idToken: string,
): Promise<AxiosResponse<AccountDto>> {
  return api.post<AccountDto>(`${API_URL}/accounts/google`, {idToken});
}

export function loginWithLinkedin(
  code: string,
): Promise<AxiosResponse<AccountDto>> {
  return api.post<AccountDto>(`${API_URL}/accounts/linkedin`, {
    authorizationCode: code,
  });
}
