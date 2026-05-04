import React from 'react';
import {render, screen, fireEvent, act} from '@testing-library/react-native';
import {Text, TouchableOpacity} from 'react-native';
import {useSocialAuth} from '@/hooks/useSocialAuth';
import {useAppSelector} from '@/store/hook';
import SocialLoginButtons from '@/components/auth/SocialLoginButtons';

const MockTouchableOpacity = TouchableOpacity;
const MockText = Text;

jest.mock('@/hooks/useSocialAuth', () => ({
  useSocialAuth: jest.fn(),
}));

jest.mock('@/store/hook', () => ({
  useAppSelector: jest.fn(),
}));

jest.mock('@/components/auth/SocialButton', () => ({
  SocialButton: function MockSocialButton({
    label,
    onPress,
    disabled,
    loading,
  }: {
    label: string;
    onPress: () => void;
    disabled?: boolean;
    loading?: boolean;
  }) {
    return (
      <MockTouchableOpacity
        onPress={onPress}
        disabled={disabled}
        testID={`btn-${label}`}>
        <MockText>
          {label} {loading ? 'Loading' : ''}
        </MockText>
      </MockTouchableOpacity>
    );
  },
}));

describe('SocialLoginButtons Integration', () => {
  const mockLoginWithGoogle = jest.fn();
  const mockLoginWithLinkedin = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    jest.useFakeTimers();

    (useSocialAuth as jest.Mock).mockReturnValue({
      loginWithGoogle: mockLoginWithGoogle,
      loginWithLinkedin: mockLoginWithLinkedin,
    });

    (useAppSelector as jest.Mock).mockReturnValue(false);
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  it('renders Google and LinkedIn buttons', () => {
    render(<SocialLoginButtons />);
    expect(screen.getByTestId('btn-Google')).toBeTruthy();
    expect(screen.getByTestId('btn-LinkedIn')).toBeTruthy();
  });

  it('calls login functions directly when legal is complete', () => {
    render(<SocialLoginButtons legalAccepted={{terms: true, privacy: true}} />);

    fireEvent.press(screen.getByTestId('btn-Google'));
    expect(mockLoginWithGoogle).toHaveBeenCalledTimes(1);

    fireEvent.press(screen.getByTestId('btn-LinkedIn'));
    expect(mockLoginWithLinkedin).toHaveBeenCalledTimes(1);

    expect(screen.queryByText(/Acceptă/)).toBeNull();
  });

  it('shows tooltip for MISSING BOTH when legal is completely empty', () => {
    render(<SocialLoginButtons legalAccepted={undefined} />);

    fireEvent.press(screen.getByTestId('btn-Google'));

    expect(mockLoginWithGoogle).not.toHaveBeenCalled();

    expect(
      screen.getByText(
        'Acceptă Termenii și Politica de confidențialitate pentru a continua.',
      ),
    ).toBeTruthy();
  });

  it('shows tooltip for MISSING PRIVACY when only terms are accepted', () => {
    render(
      <SocialLoginButtons legalAccepted={{terms: true, privacy: false}} />,
    );

    fireEvent.press(screen.getByTestId('btn-LinkedIn'));

    expect(mockLoginWithLinkedin).not.toHaveBeenCalled();
    expect(
      screen.getByText(
        'Acceptă Politica de confidențialitate pentru a continua.',
      ),
    ).toBeTruthy();
  });

  it('animates the tooltip in and out correctly', () => {
    render(
      <SocialLoginButtons legalAccepted={{terms: false, privacy: false}} />,
    );

    fireEvent.press(screen.getByTestId('btn-Google'));
    expect(screen.getByText(/Acceptă/)).toBeTruthy();

    act(() => {
      jest.advanceTimersByTime(3100);
    });

    expect(screen.queryByText(/Acceptă/)).toBeNull();
  });
});
