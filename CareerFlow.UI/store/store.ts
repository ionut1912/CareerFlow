import {configureStore} from '@reduxjs/toolkit';
import authReducer from '@/store/auth/slice';
import appReducer from '@/store/app/slice';
import userProfileReducer from '@/store/userProfile/slice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    app: appReducer,
    userProfile: userProfileReducer,
  },
});
