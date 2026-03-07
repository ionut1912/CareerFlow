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
import {registerThunk} from '@/store/auth/thunks';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {useRouter} from 'expo-router';
import RegisterScreen from '@/app/(auth)/register';

jest.mock('expo-router', () => ({
  useRouter: jest.fn(),
  usePathname: jest.fn(() => '/register'),
}));

jest.mock('@/utils/toast', () => ({
  showErrorToast: jest.fn(),
  showSuccessToast: jest.fn(),
}));

jest.mock('@/store/auth/thunks', () => ({
  registerThunk: jest.fn(),
}));

jest.mock('@/hooks/useLegalAcceptance', () => ({
  useLegalAcceptance: () => ({
    legalAccepted: true,
    isLegalComplete: true,
    onAccept: jest.fn(),
    onReject: jest.fn(),
  }),
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

describe('RegisterScreen Integration', () => {
  const mockReplace = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
  });

  it('renders initial UI elements correctly', () => {
    renderWithRedux(<RegisterScreen />);

    expect(screen.getByText('Acceseaza Career Flow')).toBeTruthy();
    expect(screen.getByPlaceholderText('John Doe')).toBeTruthy();
    expect(screen.getByPlaceholderText('jdoe')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();

    const submitTexts = screen.getAllByText('Creare cont');
    expect(submitTexts[submitTexts.length - 1]).toBeTruthy();
  });

  it('navigates to the login screen when footer action is pressed', () => {
    renderWithRedux(<RegisterScreen />);

    const authTexts = screen.getAllByText('Autentificare');
    fireEvent.press(authTexts[authTexts.length - 1]);

    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
  });

  it('handles a successful registration flow', async () => {
    jest.useFakeTimers();

    (registerThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.resolve({id: 1, name: 'John Doe'}),
    }));

    renderWithRedux(<RegisterScreen />);

    fireEvent.changeText(screen.getByPlaceholderText('John Doe'), 'Test User');
    fireEvent.changeText(screen.getByPlaceholderText('jdoe'), 'testuser');
    fireEvent.changeText(
      screen.getByPlaceholderText('you@example.com'),
      'test@example.com',
    );
    fireEvent.changeText(
      screen.getByPlaceholderText('Parola'),
      'SecurePass123!',
    );
    fireEvent.changeText(
      screen.getByPlaceholderText('Confirma parola'),
      'SecurePass123!',
    );

    const submitTexts = screen.getAllByText('Creare cont');
    const submitButton = submitTexts[submitTexts.length - 1];

    fireEvent.press(submitButton);

    expect(registerThunk).toHaveBeenCalledWith({
      name: 'Test User',
      username: 'testuser',
      email: 'test@example.com',
      password: 'SecurePass123!',
      confirmPassword: 'SecurePass123!',
    });

    await waitFor(() => {
      expect(showSuccessToast).toHaveBeenCalledWith(
        'Cont creat cu succes!',
        'Te rugam sa te autentifici.',
      );
    });

    await act(async () => {
      jest.advanceTimersByTime(1500);
    });

    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    jest.useRealTimers();
  });

  it('handles a failed registration flow', async () => {
    const mockError = new Error('Email already exists');

    (registerThunk as unknown as jest.Mock).mockReturnValue(() => ({
      unwrap: () => Promise.reject(mockError),
    }));

    renderWithRedux(<RegisterScreen />);

    fireEvent.changeText(screen.getByPlaceholderText('John Doe'), 'Test User');
    fireEvent.changeText(screen.getByPlaceholderText('jdoe'), 'testuser');
    fireEvent.changeText(
      screen.getByPlaceholderText('you@example.com'),
      'exist@example.com',
    );
    fireEvent.changeText(
      screen.getByPlaceholderText('Parola'),
      'SecurePass123!',
    );
    fireEvent.changeText(
      screen.getByPlaceholderText('Confirma parola'),
      'SecurePass123!',
    );

    const submitTexts = screen.getAllByText('Creare cont');
    const submitButton = submitTexts[submitTexts.length - 1];

    fireEvent.press(submitButton);

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare la inregistrare',
        mockError,
      );
    });

    expect(mockReplace).not.toHaveBeenCalled();
  });
});
