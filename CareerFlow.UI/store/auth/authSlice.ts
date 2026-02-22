import {createSlice, PayloadAction} from '@reduxjs/toolkit';
import {AuthState} from './models';
import {AccountDto} from '@/models/auth.models';
import {
  loginThunk,
  loginWithGoogleThunk,
  loginWithLinkedinThunk,
  registerThunk,
} from './thunks';

const initialState: AuthState = {
  account: null,
  loading: false,
  error: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.account = null;
    },
    clearError(state) {
      state.error = null;
    },
  },
  extraReducers: builder => {
    builder
      .addCase(registerThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(loginThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        loginThunk.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.account = action.payload;
        },
      )
      .addCase(loginThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(loginWithGoogleThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        loginWithGoogleThunk.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.account = action.payload;
        },
      )
      .addCase(loginWithGoogleThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(loginWithLinkedinThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        loginWithLinkedinThunk.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.account = action.payload;
        },
      )
      .addCase(loginWithLinkedinThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const {logout, clearError} = authSlice.actions;
export default authSlice.reducer;
