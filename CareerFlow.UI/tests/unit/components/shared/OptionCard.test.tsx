import React from 'react';
import {render, fireEvent} from '@testing-library/react-native';
import {OptionCard} from '@/components/shared/OptionCard';

// 1. Mock the vector icons to prevent native rendering errors in Jest
jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: 'MaterialIcons',
}));

// 2. Mock the theme colors (Optional, but good if your theme object is complex)
jest.mock('@/constants/theme', () => ({
  COLORS: {
    primary: '#primary',
    textMuted: '#textMuted',
    inputBg: '#inputBg',
    border: '#border',
    primaryWash: '#primaryWash',
    primaryBorder: '#primaryBorder',
    text: '#text',
  },
}));

describe('OptionCard Component', () => {
  // Setup a standard mock item to use across tests
  const mockItem = {
    id: '1', // Assuming your OptionType has an ID
    title: 'Notification Settings',
    desc: 'Manage your daily alerts',
    icon: 'notifications',
  };

  const mockOnPress = jest.fn();

  // Reset the mock function before every test so counts don't bleed over
  beforeEach(() => {
    mockOnPress.mockClear();
  });

  it('renders the title and description correctly', () => {
    const {getByText} = render(
      <OptionCard
        item={mockItem}
        isSelected={false}
        isMulti={false}
        onPress={mockOnPress}
      />,
    );

    expect(getByText('Notification Settings')).toBeTruthy();
    expect(getByText('Manage your daily alerts')).toBeTruthy();
  });

  it('calls onPress when the card is tapped', () => {
    const {getByText} = render(
      <OptionCard
        item={mockItem}
        isSelected={false}
        isMulti={false}
        onPress={mockOnPress}
      />,
    );

    const cardElement = getByText('Notification Settings').parent; // Get the TouchableOpacity wrapper
    fireEvent.press(cardElement);

    expect(mockOnPress).toHaveBeenCalledTimes(1);
  });

  describe('Single Selection Mode (isMulti = false)', () => {
    it('renders the unselected radio icon when isSelected is false', () => {
      const {root} = render(
        <OptionCard
          item={mockItem}
          isSelected={false}
          isMulti={false}
          onPress={mockOnPress}
        />,
      );

      // Find the specific MaterialIcon used for the checkmark
      const checkIcon = root.findAllByType('MaterialIcons')[1];
      expect(checkIcon.props.name).toBe('radio-button-unchecked');
    });

    it('renders the selected radio icon when isSelected is true', () => {
      const {root} = render(
        <OptionCard
          item={mockItem}
          isSelected={true}
          isMulti={false}
          onPress={mockOnPress}
        />,
      );

      const checkIcon = root.findAllByType('MaterialIcons')[1];
      expect(checkIcon.props.name).toBe('check-circle');
    });
  });

  describe('Multi Selection Mode (isMulti = true)', () => {
    it('renders the unselected checkbox icon when isSelected is false', () => {
      const {root} = render(
        <OptionCard
          item={mockItem}
          isSelected={false}
          isMulti={true}
          onPress={mockOnPress}
        />,
      );

      const checkIcon = root.findAllByType('MaterialIcons')[1];
      expect(checkIcon.props.name).toBe('check-box-outline-blank');
    });

    it('renders the selected checkbox icon when isSelected is true', () => {
      const {root} = render(
        <OptionCard
          item={mockItem}
          isSelected={true}
          isMulti={true}
          onPress={mockOnPress}
        />,
      );

      const checkIcon = root.findAllByType('MaterialIcons')[1];
      expect(checkIcon.props.name).toBe('check-box');
    });
  });
});
