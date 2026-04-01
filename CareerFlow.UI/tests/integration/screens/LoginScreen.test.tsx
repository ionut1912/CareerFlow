import React from 'react';
import {
  render,
  screen,
  fireEvent,
  waitFor,
} from '@testing-library/react-native';
import {Provider} from 'react-redux';
import {configureStore} from '@reduxjs/toolkit';
import {loginThunk} from '@/store/auth/thunks';
import {showErrorToast} from '@/utils/toast';
import {useRouter} from 'expo-router';
import LoginScreen from '@/app/(auth)/login';

jest.mock('@/utils/toast', () => ({
  showErrorToast: jest.fn(),
}));

jest.mock('@/store/auth/thunks', () => ({
  loginThunk: jest.fn(),
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

describe('LoginScreen Integration', () => {
  const mockReplace = jest.fn();
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      replace: mockReplace,
      push: mockPush,
    });
  });

  it('renders initial UI elements correctly', () => {
    renderWithRedux(<LoginScreen />);
    expect(screen.getByText('Career Flow')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();
    expect(screen.getByPlaceholderText('••••••••')).toBeTruthy();
    const authTexts = screen.getAllByText('Autentificare');
    expect(authTexts[authTexts.length - 1]).toBeTruthy();
  });

  it('navigates to the register screen when footer action is pressed', () => {
    renderWithRedux(<LoginScreen />);
    fireEvent.press(screen.getByText('Inregistreaza-te'));
    expect(mockReplace).toHaveBeenCalledWith('/(auth)/register');
  });

  it('navigates to forgot password screen when link is pressed', () => {
    renderWithRedux(<LoginScreen />);
    fireEvent.press(screen.getByText('Ai uitat parola?'));
    expect(mockPush).toHaveBeenCalledWith('/(auth)/forgot-password');
  });

  it('handles a successful login flow', async () => {
    (loginThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.resolve({user: 'John Doe'}),
    }));

    renderWithRedux(<LoginScreen />);
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    const authTexts = screen.getAllByText('Autentificare');
    const submitButton = authTexts[authTexts.length - 1];

    fireEvent.changeText(emailInput, 'user@example.com');
    fireEvent.changeText(passwordInput, 'securepassword123');
    fireEvent.press(submitButton);

    await waitFor(() => {
      expect(loginThunk).toHaveBeenCalledWith({
        email: 'user@example.com',
        password: 'securepassword123',
      });
    });

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(auth)/preferences');
    });
  });

  it('handles a failed login flow and shows an error toast', async () => {
    const mockError = new Error('Invalid credentials');
    (loginThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.reject(mockError),
    }));

    renderWithRedux(<LoginScreen />);
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    const authTexts = screen.getAllByText('Autentificare');
    const submitButton = authTexts[authTexts.length - 1];

    fireEvent.changeText(emailInput, 'user@example.com');
    fireEvent.changeText(passwordInput, 'wrongpassword');
    fireEvent.press(submitButton);

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare la autentificare',
        mockError,
      );
    });

    expect(mockReplace).not.toHaveBeenCalledWith('/(auth)/preferences');
  });
});
