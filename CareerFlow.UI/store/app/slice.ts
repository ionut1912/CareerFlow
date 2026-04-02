import {createSlice} from '@reduxjs/toolkit';
import {AppState} from './models';
import {completeOnboardingThunk, initializeAppStatusThunk} from './thunks';

const initialState: AppState = {
  hasSeenOnboarding: false,
  isAppReady: false,
  error: '',
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
    builder.addCase(initializeAppStatusThunk.rejected, (state, action) => {
      state.isAppReady = true;
      state.hasSeenOnboarding = false;
      state.error = action.error.message || 'Initialization failed';
    });
    builder.addCase(completeOnboardingThunk.fulfilled, state => {
      state.hasSeenOnboarding = true;
    });
  },
});

export default appSlice.reducer;
