import axios, { AxiosInstance, InternalAxiosRequestConfig } from "axios";

export const createAuthAxios = (baseURL: string): AxiosInstance => {
  const api = axios.create({ baseURL });

  api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    if ("requires-auth" in config.headers) {
      delete (config.headers as Record<string, unknown>)["requires-auth"];
      const token = localStorage.getItem("jwt");
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  });

  return api;
};