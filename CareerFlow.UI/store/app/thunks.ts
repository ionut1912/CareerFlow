import {createAsyncThunk} from '@reduxjs/toolkit';
import AsyncStorage from '@react-native-async-storage/async-storage';

const ONBOARDING_KEY = '@career_flow_has_seen_onboarding';

export const initializeAppStatusThunk = createAsyncThunk(
  'app/initializeStatus',
  async () => {
    try {
      const value = await AsyncStorage.getItem(ONBOARDING_KEY);
      return value === 'true';
    } catch (e) {
      console.warn('Failed to fetch onboarding status', e);
      return false;
    }
  },
);

export const completeOnboardingThunk = createAsyncThunk(
  'app/completeOnboarding',
  async () => {
    try {
      await AsyncStorage.setItem(ONBOARDING_KEY, 'true');
      return true;
    } catch (e) {
      console.warn('Failed to save onboarding status', e);
      return false;
    }
  },
);

export const resetOnboardingThunk = createAsyncThunk(
  'app/resetOnboarding',
  async () => {
    try {
      await AsyncStorage.removeItem(ONBOARDING_KEY);
      return false;
    } catch (e) {
      console.warn('Failed to reset onboarding status', e);
      return false;
    }
  },
);
