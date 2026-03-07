import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {Text} from 'react-native';
import {PasswordVisibilityToggle} from '@/components/auth/PasswordVisibilityToggle';

type IconProps = {name: string; testID?: string};

const MockIcon = (props: IconProps) => (
  <Text testID={props.testID}>{props.name}</Text>
);

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: (props: IconProps) => <MockIcon {...props} />,
  Ionicons: (props: IconProps) => <MockIcon {...props} />,
  MaterialCommunityIcons: (props: IconProps) => <MockIcon {...props} />,
}));

describe('PasswordVisibilityToggle Unit Tests', () => {
  it('renders correctly when password is NOT visible', () => {
    render(<PasswordVisibilityToggle isVisible={false} onToggle={jest.fn()} />);

    expect(screen.getByText('visibility-off')).toBeTruthy();
    expect(screen.getByRole('button', {name: 'Arata parola'})).toBeTruthy();
  });

  it('renders correctly when password IS visible', () => {
    render(<PasswordVisibilityToggle isVisible={true} onToggle={jest.fn()} />);

    expect(screen.getByText('visibility')).toBeTruthy();
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
