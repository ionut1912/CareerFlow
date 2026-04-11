import {createAuthAxios} from '@/utils/authutis';

export const API_URL = 'https://carerflow-api.ro';
//for locakl testing
//export const API_URL = 'http://192.168.0.157:5247';
export const api = createAuthAxios(API_URL);
