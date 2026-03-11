import {createAsyncThunk} from '@reduxjs/toolkit';
import {MMKVLoader} from 'react-native-mmkv-storage';

export const storage = new MMKVLoader().initialize();
const ONBOARDING_KEY = '@career_flow_has_seen_onboarding';

export const initializeAppStatusThunk = createAsyncThunk(
  'app/initializeStatus',
  async () => {
    const value = storage.getBool(ONBOARDING_KEY);
    return value === true;
  },
);

export const completeOnboardingThunk = createAsyncThunk(
  'app/completeOnboarding',
  async () => {
    storage.setBool(ONBOARDING_KEY, true);
    return true;
  },
);

export const resetOnboardingThunk = createAsyncThunk(
  'app/resetOnboarding',
  async () => {
    storage.removeItem(ONBOARDING_KEY);
    return false;
  },
);
