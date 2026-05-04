import {createSlice, PayloadAction} from '@reduxjs/toolkit';
import {AuthState} from './models';
import {AccountDto} from '@/models/auth.models';
import {
  loginThunk,
  loginWithSocialThunk,
  logoutThunk,
  registerThunk,
  requestPasswordResetThunk,
  resetPasswordThunk,
  restoreSessionThunk,
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
      .addCase(loginWithSocialThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        loginWithSocialThunk.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.account = action.payload;
        },
      )
      .addCase(loginWithSocialThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(restoreSessionThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        restoreSessionThunk.fulfilled,
        (state, action: PayloadAction<AccountDto>) => {
          state.loading = false;
          state.account = action.payload;
        },
      )
      .addCase(restoreSessionThunk.rejected, state => {
        state.loading = false;
        state.account = null;
      })
      .addCase(logoutThunk.fulfilled, state => {
        state.account = null;
      })
      .addCase(requestPasswordResetThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(requestPasswordResetThunk.fulfilled, state => {
        state.loading = false;
        state.error = null;
      })
      .addCase(requestPasswordResetThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(resetPasswordThunk.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(resetPasswordThunk.fulfilled, state => {
        state.loading = false;
        state.error = null;
      })
      .addCase(resetPasswordThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const {logout, clearError} = authSlice.actions;
export default authSlice.reducer;
