import {createAuthAxios} from '@/utils/authutis';

export const API_URL = 'https://carerflow-api.ro';
export const api = createAuthAxios(API_URL);
