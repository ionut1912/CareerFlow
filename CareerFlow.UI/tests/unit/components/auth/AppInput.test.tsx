import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {AppInput} from '@/components/auth/AppInput';
import {TouchableOpacity, Text} from 'react-native';

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: 'MaterialIcons',
}));

jest.mock('@/components/auth/PasswordVisibilityToggle', () => {
  return {
    PasswordVisibilityToggle: ({onToggle}: {onToggle: () => void}) => (
      <TouchableOpacity onPress={onToggle} testID="password-toggle">
        <Text>Toggle</Text>
      </TouchableOpacity>
    ),
  };
});

describe('AppInput Unit Tests', () => {
  it('renders correctly with basic props', () => {
    render(
      <AppInput
        label="Adresa de email"
        icon="mail-outline"
        placeholder="you@example.com"
      />,
    );

    expect(screen.getByText('Adresa de email')).toBeTruthy();
    expect(screen.getByPlaceholderText('you@example.com')).toBeTruthy();
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('handles user typing and calls onChangeText', () => {
    const mockOnChangeText = jest.fn();
    render(
      <AppInput
        label="Nume"
        icon="person"
        placeholder="Numele tau"
        onChangeText={mockOnChangeText}
      />,
    );

    const input = screen.getByPlaceholderText('Numele tau');
    fireEvent.changeText(input, 'John Doe');

    expect(mockOnChangeText).toHaveBeenCalledWith('John Doe');
    expect(mockOnChangeText).toHaveBeenCalledTimes(1);
  });

  it('shows an error message only when touched AND error is provided', () => {
    const errorMessage = 'Email-ul este invalid';

    const {rerender} = render(
      <AppInput
        label="Email"
        icon="mail"
        error={errorMessage}
        touched={false}
      />,
    );
    expect(screen.queryByText(errorMessage)).toBeNull();

    rerender(
      <AppInput
        label="Email"
        icon="mail"
        error={errorMessage}
        touched={true}
      />,
    );
    expect(screen.getByText(errorMessage)).toBeTruthy();
    expect(screen.getByRole('alert')).toBeTruthy();
  });

  it('toggles secureTextEntry when used as a password input', () => {
    render(
      <AppInput
        label="Parola"
        icon="lock"
        placeholder="••••••••"
        isPassword={true}
        value="secret123"
      />,
    );

    const input = screen.getByPlaceholderText('••••••••');
    const toggleButton = screen.getByTestId('password-toggle');

    expect(input.props.secureTextEntry).toBe(true);

    fireEvent.press(toggleButton);

    expect(input.props.secureTextEntry).toBe(false);

    fireEvent.press(toggleButton);
    expect(input.props.secureTextEntry).toBe(true);
  });
});
