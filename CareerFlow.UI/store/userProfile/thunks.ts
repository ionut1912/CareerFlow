import {CreateUserProfileRequest} from '@/models/userProfile.models';
import {saveUserProfile} from '@/services/userProfileService';
import {createAsyncThunk} from '@reduxjs/toolkit';
import {isAxiosError} from 'axios';

export const createUserProfileThunk = createAsyncThunk(
  'userProfile/create',
  async (payload: CreateUserProfileRequest, {rejectWithValue}) => {
    try {
      const result = await saveUserProfile(payload);
      return result.data;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        return rejectWithValue(
          error.response?.data?.message || 'Failed to create user profile',
        );
      }
      console.error('Error creating user profile:', error);
      return rejectWithValue('Failed to create user profile');
    }
  },
);
