import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {TabButton} from '@/components/shared/TabButton';

describe('TabButton Unit Tests', () => {
  it('renders the title correctly', () => {
    render(
      <TabButton title="Autentificare" active={false} onPress={jest.fn()} />,
    );

    expect(screen.getByText('Autentificare')).toBeTruthy();
  });

  it('calls onPress when the tab is pressed', () => {
    const mockOnPress = jest.fn();
    render(
      <TabButton title="Inregistrare" active={false} onPress={mockOnPress} />,
    );

    const button = screen.getByText('Inregistrare');
    fireEvent.press(button);

    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });

  it('renders correctly when in the active state', () => {
    const {getByText} = render(
      <TabButton title="Active Tab" active={true} onPress={jest.fn()} />,
    );

    // The primary thing to test is that it renders the text without crashing
    // when applying the conditional 'active' styles.
    expect(getByText('Active Tab')).toBeTruthy();
  });
});
