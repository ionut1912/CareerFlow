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
import {View} from 'react-native';

jest.mock('@expo/vector-icons', () => {
  return {
    MaterialIcons: View,
    MaterialCommunityIcons: View,
    Ionicons: View,
    Feather: View,
    FontAwesome: View,
  };
});

jest.mock('expo-router', () => ({
  useRouter: jest.fn(),
  usePathname: jest.fn(() => '/reset-password'),
  useLocalSearchParams: jest.fn(),
}));

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
  const mockToken = 'abc-123-secure-token';

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
    (useLocalSearchParams as jest.Mock).mockReturnValue({token: mockToken});
  });

  it('renders initial UI elements correctly', () => {
    renderWithRedux(<ResetPasswordScreen />);

    expect(screen.getByText('Resetare Parolă')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();

    const passwordInputs = screen.getAllByPlaceholderText('••••••••');
    expect(passwordInputs.length).toBe(2);

    const submitTexts = screen.getAllByText('Resetează Parola');
    expect(submitTexts[submitTexts.length - 1]).toBeTruthy();
  });

  it('handles a successful password reset flow', async () => {
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
    fireEvent.changeText(passwordInputs[0], 'NewSecurePass123!');
    fireEvent.changeText(passwordInputs[1], 'NewSecurePass123!');

    const submitTexts = screen.getAllByText('Resetează Parola');
    const submitButton = submitTexts[submitTexts.length - 1];

    fireEvent.press(submitButton);

    expect(resetPasswordThunk).toHaveBeenCalledWith({
      email: 'test@example.com',
      newPassword: 'NewSecurePass123!',
      token: mockToken,
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

  it('handles a failed password reset flow', async () => {
    const mockError = new Error('Token expired');

    (resetPasswordThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.reject(mockError),
    }));

    renderWithRedux(<ResetPasswordScreen />);

    fireEvent.changeText(
      screen.getByPlaceholderText('you@example.com'),
      'test@example.com',
    );

    const passwordInputs = screen.getAllByPlaceholderText('••••••••');
    fireEvent.changeText(passwordInputs[0], 'NewSecurePass123!');
    fireEvent.changeText(passwordInputs[1], 'NewSecurePass123!');

    const submitTexts = screen.getAllByText('Resetează Parola');
    const submitButton = submitTexts[submitTexts.length - 1];

    fireEvent.press(submitButton);

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare la resetare',
        mockError,
      );
    });

    expect(mockReplace).not.toHaveBeenCalled();
  });
});
