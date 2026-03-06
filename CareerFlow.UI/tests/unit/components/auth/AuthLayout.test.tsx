import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {Text, TouchableOpacity, View} from 'react-native';
import {useRouter, usePathname} from 'expo-router';
import {useLegalModal} from '@/hooks/useLegalModal';
import {AuthLayout} from '@/components/auth/AuthLayout';

jest.mock('expo-router', () => ({
  useRouter: jest.fn(),
  usePathname: jest.fn(),
}));

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: 'MaterialIcons',
}));

jest.mock('@/components/auth/SocialLoginButtons', () => {
  return {
    __esModule: true,
    default: function MockSocialButtons() {
      return <Text>MockedSocialButtons</Text>;
    },
  };
});

jest.mock('@/components/legal/LegalModal', () => {
  return {
    LegalModal: function MockLegalModal() {
      return <View testID="mock-legal-modal" />;
    },
  };
});

jest.mock('@/components/shared/TabButton', () => {
  return {
    TabButton: function MockTabButton({
      title,
      onPress,
    }: {
      title: string;
      onPress: () => void;
    }) {
      return (
        <TouchableOpacity onPress={onPress}>
          <Text>{title}</Text>
        </TouchableOpacity>
      );
    },
  };
});

jest.mock('@/hooks/useLegalModal', () => ({
  useLegalModal: jest.fn(),
}));

describe('AuthLayout Unit Tests', () => {
  const mockReplace = jest.fn();
  const mockOpenModal = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
    (usePathname as jest.Mock).mockReturnValue('/(auth)/login');

    (useLegalModal as jest.Mock).mockReturnValue({
      modal: {visible: false, title: '', content: '', loading: false},
      open: mockOpenModal,
      close: jest.fn(),
      handleAccept: jest.fn(),
      handleReject: jest.fn(),
    });
  });

  it('renders children and basic text props correctly', () => {
    render(
      <AuthLayout title="Test Title" subtitle="Test Subtitle">
        <Text>Child Component</Text>
      </AuthLayout>,
    );

    expect(screen.getByText('Test Title')).toBeTruthy();
    expect(screen.getByText('Test Subtitle')).toBeTruthy();
    expect(screen.getByText('Child Component')).toBeTruthy();
  });

  it('hides tabs, social auth, and legal links when props are false', () => {
    render(
      <AuthLayout
        title="Title"
        subtitle="Subtitle"
        showTabs={false}
        showSocialAuth={false}
        showLegalLinks={false}>
        <Text>Child</Text>
      </AuthLayout>,
    );

    expect(screen.queryByText('Inregistrare')).toBeNull();
    expect(screen.queryByText('SAU CONTINUA CU')).toBeNull();
    expect(screen.queryByText('MockedSocialButtons')).toBeNull();
    expect(screen.queryByText('Termeni și condiții')).toBeNull();
  });

  it('handles routing when tab buttons are pressed', () => {
    render(
      <AuthLayout title="Title" subtitle="Subtitle">
        <Text>Child</Text>
      </AuthLayout>,
    );

    fireEvent.press(screen.getByText('Inregistrare'));
    expect(mockReplace).toHaveBeenCalledWith('/(auth)/register');

    fireEvent.press(screen.getByText('Autentificare'));
    expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
  });

  it('renders footer text and handles footer action', () => {
    const mockFooterAction = jest.fn();
    render(
      <AuthLayout
        title="Title"
        subtitle="Subtitle"
        footerText="Don't have an account?"
        footerActionText="Sign Up"
        onFooterAction={mockFooterAction}>
        <Text>Child</Text>
      </AuthLayout>,
    );

    const actionButton = screen.getByText('Sign Up');
    expect(actionButton).toBeTruthy();

    fireEvent.press(actionButton);
    expect(mockFooterAction).toHaveBeenCalledTimes(1);
  });

  it('opens the legal modal when legal links are pressed', () => {
    render(
      <AuthLayout title="Title" subtitle="Subtitle">
        <Text>Child</Text>
      </AuthLayout>,
    );

    fireEvent.press(screen.getByText('Politica de confidențialitate'));
    expect(mockOpenModal).toHaveBeenCalledWith('privacy');

    fireEvent.press(screen.getByText('Termeni și condiții'));
    expect(mockOpenModal).toHaveBeenCalledWith('terms');
  });
});
