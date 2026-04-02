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
import {requestPasswordResetThunk} from '@/store/auth/thunks';
import {showSuccessToast, showErrorToast} from '@/utils/toast';
import {useRouter} from 'expo-router';
import ForgotPasswordScreen from '@/app/(auth)/forgot-password';

jest.mock('@/utils/toast', () => ({
  showSuccessToast: jest.fn(),
  showErrorToast: jest.fn(),
}));

jest.mock('@/store/auth/thunks', () => ({
  requestPasswordResetThunk: jest.fn(),
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

describe('ForgotPasswordScreen Integration', () => {
  const mockReplace = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
  });

  it('renders correctly with initial UI elements', () => {
    renderWithRedux(<ForgotPasswordScreen />);
    expect(screen.getByText('Ai uitat parola?')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();
    expect(screen.getByText('Trimite link de resetare')).toBeTruthy();
  });

  it('navigates back to login when footer action is pressed', () => {
    renderWithRedux(<ForgotPasswordScreen />);
    fireEvent.press(screen.getByText('Înapoi la Login'));
    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
  });

  it('handles successful password reset flow', async () => {
    jest.useFakeTimers();
    (requestPasswordResetThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.resolve(),
    }));

    renderWithRedux(<ForgotPasswordScreen />);
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const submitButton = screen.getByText('Trimite link de resetare');

    fireEvent.changeText(emailInput, 'test@example.com');
    fireEvent.press(submitButton);

    expect(requestPasswordResetThunk).toHaveBeenCalledWith({
      email: 'test@example.com',
    });

    await waitFor(() => {
      expect(showSuccessToast).toHaveBeenCalledWith(
        'Email trimis!',
        'Verifică-ți căsuța de email pentru link-ul de resetare.',
      );
    });

    await act(async () => {
      jest.advanceTimersByTime(2000);
    });

    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    jest.useRealTimers();
  });

  it('handles rejected password reset flow', async () => {
    const mockError = new Error('Network Error');
    (requestPasswordResetThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.reject(mockError),
    }));

    renderWithRedux(<ForgotPasswordScreen />);
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const submitButton = screen.getByText('Trimite link de resetare');

    fireEvent.changeText(emailInput, 'fail@example.com');
    fireEvent.press(submitButton);

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare',
        mockError,
        'Nu am putut trimite email-ul.',
      );
    });

    expect(mockReplace).not.toHaveBeenCalled();
  });
});
