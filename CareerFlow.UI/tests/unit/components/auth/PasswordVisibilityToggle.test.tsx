import React from 'react';
import {Text} from 'react-native';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {PasswordVisibilityToggle} from '@/components/auth/PasswordVisibilityToggle';

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: function MockMaterialIcons({name}: {name: string}) {
    return <Text testID={`icon-${name}`}>{name}</Text>;
  },
}));

describe('PasswordVisibilityToggle Unit Tests', () => {
  it('renders correctly when password is NOT visible', () => {
    render(<PasswordVisibilityToggle isVisible={false} onToggle={jest.fn()} />);

    expect(screen.getByTestId('icon-visibility-off')).toBeTruthy();
    expect(screen.getByRole('button', {name: 'Arata parola'})).toBeTruthy();
  });

  it('renders correctly when password IS visible', () => {
    render(<PasswordVisibilityToggle isVisible={true} onToggle={jest.fn()} />);

    expect(screen.getByTestId('icon-visibility')).toBeTruthy();
    expect(screen.getByRole('button', {name: 'Ascunde parola'})).toBeTruthy();
  });

  it('calls the onToggle handler when pressed', () => {
    const mockOnToggle = jest.fn();
    render(
      <PasswordVisibilityToggle isVisible={false} onToggle={mockOnToggle} />,
    );

    const toggleButton = screen.getByRole('button');
    fireEvent.press(toggleButton);

    expect(mockOnToggle).toHaveBeenCalledTimes(1);
  });
});
