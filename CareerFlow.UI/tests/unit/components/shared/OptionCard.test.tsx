import React from 'react';
import {render, fireEvent} from '@testing-library/react-native';
import {OptionCard} from '@/components/shared/OptionCard';

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: 'MaterialIcons',
}));

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
  const mockItem = {
    id: '1',
    title: 'Notification Settings',
    desc: 'Manage your daily alerts',
    icon: 'notifications',
  };

  const mockOnPress = jest.fn();

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

    expect(
      getByText('Notification Settings', {includeHiddenElements: true}),
    ).toBeTruthy();
    expect(
      getByText('Manage your daily alerts', {includeHiddenElements: true}),
    ).toBeTruthy();
  });

  it('calls onPress when the card is tapped', () => {
    const {getByLabelText} = render(
      <OptionCard
        item={mockItem}
        isSelected={false}
        isMulti={false}
        onPress={mockOnPress}
      />,
    );

    const cardElement = getByLabelText(
      'Notification Settings: Manage your daily alerts',
    );
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
