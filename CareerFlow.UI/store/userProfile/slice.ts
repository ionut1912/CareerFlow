import {createSlice} from '@reduxjs/toolkit';
import {UserProfileState} from './models';
import {createUserProfileThunk} from './thunks';

const initialState: UserProfileState = {
  profileId: null,
  error: null,
  loading: false,
};

const userProfileSlice = createSlice({
  name: 'userProfile',
  initialState, // Using shorthand since the variable name now matches the property name
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(createUserProfileThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createUserProfileThunk.fulfilled, (state, action) => {
        state.loading = false;
        state.profileId = action.payload;
        state.error = null;
      })
      .addCase(createUserProfileThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export default userProfileSlice.reducer;
