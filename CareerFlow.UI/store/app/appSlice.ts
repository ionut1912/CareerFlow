import {createSlice} from '@reduxjs/toolkit';
import {AppState} from './models';
import {
  completeOnboardingThunk,
  initializeAppStatusThunk,
  resetOnboardingThunk,
} from './thunks';

const initialState: AppState = {
  hasSeenOnboarding: false,
  isAppReady: false,
};

const appSlice = createSlice({
  name: 'app',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder.addCase(initializeAppStatusThunk.fulfilled, (state, action) => {
      state.hasSeenOnboarding = action.payload;
      state.isAppReady = true;
    });
    builder.addCase(initializeAppStatusThunk.rejected, state => {
      state.isAppReady = true;
    });
    builder.addCase(completeOnboardingThunk.fulfilled, state => {
      state.hasSeenOnboarding = true;
    });
    builder.addCase(resetOnboardingThunk.fulfilled, state => {
      state.hasSeenOnboarding = false;
    });
  },
});

export default appSlice.reducer;
