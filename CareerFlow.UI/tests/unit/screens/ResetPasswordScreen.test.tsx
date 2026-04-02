import React from 'react';
import {
  render,
  screen,
  fireEvent,
  waitFor,
  act,
} from '@testing-library/react-native';
import {Provider} from 'react-redux';
import {configureStore} from '@reduxjs/toolkit';
import {resetPasswordThunk} from '@/store/auth/thunks';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {useRouter, useLocalSearchParams} from 'expo-router';
import ResetPasswordScreen from '@/app/(auth)/reset-password';

jest.mock('@/utils/toast', () => ({
  showErrorToast: jest.fn(),
  showSuccessToast: jest.fn(),
}));

jest.mock('@/store/auth/thunks', () => ({
  resetPasswordThunk: jest.fn(),
}));

const renderWithRedux = (
  component: React.ReactElement,
  preloadedState = {auth: {loading: false}},
) => {
  const store = configureStore({
    reducer: {
      auth: (state = preloadedState.auth) => state,
    },
    preloadedState,
  });
  return render(<Provider store={store}>{component}</Provider>);
};

describe('ResetPasswordScreen Integration', () => {
  const mockReplace = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
    (useLocalSearchParams as jest.Mock).mockReturnValue({
      token: 'mock-token-123',
    });
  });

  it('renders invalid link message when no token is present', () => {
    (useLocalSearchParams as jest.Mock).mockReturnValue({});
    renderWithRedux(<ResetPasswordScreen />);
    expect(
      screen.getByText('Link-ul de resetare este invalid sau a expirat.'),
    ).toBeTruthy();
  });

  it('renders initial UI elements correctly', () => {
    renderWithRedux(<ResetPasswordScreen />);
    expect(screen.getByText('Resetare Parolă')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();
  });

  it('handles a successful reset password flow', async () => {
    jest.useFakeTimers();
    (resetPasswordThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.resolve(),
    }));

    renderWithRedux(<ResetPasswordScreen />);
    fireEvent.changeText(
      screen.getByPlaceholderText('you@example.com'),
      'test@example.com',
    );
    const passwordInputs = screen.getAllByPlaceholderText('••••••••');
    fireEvent.changeText(passwordInputs[0], 'NewPass123!');
    fireEvent.changeText(passwordInputs[1], 'NewPass123!');
    fireEvent.press(screen.getByText('Resetează Parola'));

    expect(resetPasswordThunk).toHaveBeenCalledWith({
      email: 'test@example.com',
      newPassword: 'NewPass123!',
      token: 'mock-token-123',
    });

    await waitFor(() => {
      expect(showSuccessToast).toHaveBeenCalledWith('Parolă resetată!');
    });

    await act(async () => {
      jest.advanceTimersByTime(1500);
    });

    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    jest.useRealTimers();
  });

  it('handles a failed reset password flow', async () => {
    const mockError = new Error('Token expirat');
    (resetPasswordThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.reject(mockError),
    }));

    renderWithRedux(<ResetPasswordScreen />);
    fireEvent.changeText(
      screen.getByPlaceholderText('you@example.com'),
      'test@example.com',
    );
    const passwordInputs = screen.getAllByPlaceholderText('••••••••');
    fireEvent.changeText(passwordInputs[0], 'NewPass123!');
    fireEvent.changeText(passwordInputs[1], 'NewPass123!');
    fireEvent.press(screen.getByText('Resetează Parola'));

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare la resetare',
        mockError,
      );
    });

    expect(mockReplace).not.toHaveBeenCalled();
  });
});
