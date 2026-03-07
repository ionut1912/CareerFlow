import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {Text} from 'react-native';
import {GradientButton} from '@/components/shared/GradientButton';

type IconProps = {name: string};

const MockIcon = (props: IconProps) => <Text>{props.name}</Text>;

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: (props: IconProps) => <MockIcon {...props} />,
  Ionicons: (props: IconProps) => <MockIcon {...props} />,
  FontAwesome: (props: IconProps) => <MockIcon {...props} />,
}));

describe('GradientButton Unit Tests', () => {
  it('renders correctly with basic text', () => {
    render(<GradientButton text="Autentificare" onPress={jest.fn()} />);
    expect(screen.getByText('Autentificare')).toBeTruthy();
    expect(screen.getByRole('button', {name: 'Autentificare'})).toBeTruthy();
  });

  it('calls the onPress handler when clicked', () => {
    const mockOnPress = jest.fn();
    render(<GradientButton text="Click Me" onPress={mockOnPress} />);
    const button = screen.getByRole('button');
    fireEvent.press(button);
    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });

  it('renders an icon when the icon prop is provided', () => {
    render(<GradientButton text="Trimite" onPress={jest.fn()} icon="send" />);
    expect(screen.getByText('send')).toBeTruthy();
  });

  it('does not call onPress and sets accessibility state when disabled', () => {
    const mockOnPress = jest.fn();
    render(
      <GradientButton
        text="Loading..."
        onPress={mockOnPress}
        disabled={true}
      />,
    );
    const button = screen.getByRole('button');
    fireEvent.press(button);
    expect(mockOnPress).not.toHaveBeenCalled();
    expect(button.props.accessibilityState.disabled).toBe(true);
  });
});
