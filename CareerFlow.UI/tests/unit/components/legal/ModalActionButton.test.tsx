import React from 'react';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {ModalActionButton} from '@/components/legal/ModalActionButton';

describe('ModalActionButton Unit Tests', () => {
  it('renders the accept variant correctly and triggers onPress', () => {
    const mockOnPress = jest.fn();
    render(
      <ModalActionButton
        label="Acceptă"
        variant="accept"
        onPress={mockOnPress}
      />,
    );

    // Verify text is rendered
    expect(screen.getByText('Acceptă')).toBeTruthy();

    // Verify accessibility role and label
    const button = screen.getByRole('button', {name: 'Acceptă'});
    expect(button).toBeTruthy();

    // Verify interaction
    fireEvent.press(button);
    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });

  it('renders the reject variant correctly and triggers onPress', () => {
    const mockOnPress = jest.fn();
    render(
      <ModalActionButton
        label="Refuză"
        variant="reject"
        onPress={mockOnPress}
      />,
    );

    // Verify text is rendered
    expect(screen.getByText('Refuză')).toBeTruthy();

    // Verify accessibility role and label
    const button = screen.getByRole('button', {name: 'Refuză'});
    expect(button).toBeTruthy();

    // Verify interaction
    fireEvent.press(button);
    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });
});
