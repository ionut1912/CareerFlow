import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {Text} from 'react-native';
import {SocialButton} from '@/components/auth/SocialButton';

type IconProps = {name: string; testID?: string};

const MockIconComponent = (props: IconProps) => (
  <Text testID={props.testID}>{props.name}</Text>
);

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: (props: IconProps) => <MockIconComponent {...props} />,
  Ionicons: (props: IconProps) => <MockIconComponent {...props} />,
  FontAwesome: (props: IconProps) => <MockIconComponent {...props} />,
  AntDesign: (props: IconProps) => <MockIconComponent {...props} />,
}));

describe('SocialButton Unit Tests', () => {
  it('renders correctly with label and icon', () => {
    render(
      <SocialButton
        label="Google"
        icon="google"
        onPress={jest.fn()}
        disabled={false}
        loading={false}
      />,
    );

    expect(screen.getByText('Google')).toBeTruthy();
    expect(screen.getByText('google')).toBeTruthy();
    expect(
      screen.getByRole('button', {name: 'Continuă cu Google'}),
    ).toBeTruthy();
    expect(screen.queryByLabelText('loading')).toBeNull();
  });

  it('calls onPress when clicked', () => {
    const mockOnPress = jest.fn();
    render(
      <SocialButton
        label="Apple"
        icon="apple"
        onPress={mockOnPress}
        disabled={false}
        loading={false}
      />,
    );

    const button = screen.getByRole('button');
    fireEvent.press(button);

    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });

  it('displays a loading indicator and hides text/icon when loading is true', () => {
    render(
      <SocialButton
        label="Facebook"
        icon="facebook"
        onPress={jest.fn()}
        disabled={false}
        loading={true}
      />,
    );

    expect(screen.queryByText('Facebook')).toBeNull();
    expect(screen.queryByText('facebook')).toBeNull();
  });

  it('does not trigger onPress and sets accessibility state when disabled', () => {
    const mockOnPress = jest.fn();
    render(
      <SocialButton
        label="Github"
        icon="github"
        onPress={mockOnPress}
        disabled={true}
        loading={false}
      />,
    );

    const button = screen.getByRole('button');
    fireEvent.press(button);

    expect(mockOnPress).not.toHaveBeenCalled();
    expect(button.props.accessibilityState.disabled).toBe(true);
  });
});
