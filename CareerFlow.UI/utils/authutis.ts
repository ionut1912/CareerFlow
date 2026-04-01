import {secureStorage} from '@/utils/secureStorage';
import axios, {AxiosInstance, InternalAxiosRequestConfig} from 'axios';

export const createAuthAxios = (baseURL: string): AxiosInstance => {
  const api = axios.create({baseURL});

  api.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
      if (config.headers && 'requires-auth' in config.headers) {
        delete (config.headers as Record<string, unknown>)['requires-auth'];
        const token = await secureStorage.getToken();

        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
      }
      return config;
    },
    error => {
      return Promise.reject(error);
    },
  );

  return api;
};
