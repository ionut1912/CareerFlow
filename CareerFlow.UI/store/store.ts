import {configureStore} from '@reduxjs/toolkit';
import authReducer from '@/store/auth/authSlice';
import appReducer from '@/store/app/appSlice';

export const store = configureStore({
  reducer: {auth: authReducer, app: appReducer},
});
