import {createAsyncThunk} from '@reduxjs/toolkit';
import AsyncStorage from '@react-native-async-storage/async-storage';

const ONBOARDING_KEY = '@career_flow_has_seen_onboarding';

export const initializeAppStatusThunk = createAsyncThunk(
  'app/initializeStatus',
  async () => {
    const value = await AsyncStorage.getItem(ONBOARDING_KEY);
    return value === 'true';
  },
);

export const completeOnboardingThunk = createAsyncThunk(
  'app/completeOnboarding',
  async () => {
    await AsyncStorage.setItem(ONBOARDING_KEY, 'true');
    return true;
  },
);

export const resetOnboardingThunk = createAsyncThunk(
  'app/resetOnboarding',
  async () => {
    await AsyncStorage.removeItem(ONBOARDING_KEY);
    return false;
  },
);
