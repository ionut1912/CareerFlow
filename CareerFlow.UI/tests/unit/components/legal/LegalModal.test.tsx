import React from 'react';
import {Text, TouchableOpacity} from 'react-native';
import {render, screen, fireEvent} from '@testing-library/react-native';
import {LegalModal} from '@/components/legal/LegalModal';

jest.mock('@expo/vector-icons', () => ({
  MaterialIcons: function MockMaterialIcons({name}: {name: string}) {
    return <Text testID={`icon-${name}`}>{name}</Text>;
  },
}));

jest.mock('react-native-markdown-display', () => {
  return function MockMarkdownDisplay({children}: {children: React.ReactNode}) {
    return <Text testID="mock-markdown">{children}</Text>;
  };
});

jest.mock('@/components/legal/ModalActionButton', () => ({
  ModalActionButton: function MockModalActionButton({
    label,
    onPress,
  }: {
    label: string;
    onPress: () => void;
  }) {
    return (
      <TouchableOpacity onPress={onPress} testID={`action-btn-${label}`}>
        <Text>{label}</Text>
      </TouchableOpacity>
    );
  },
}));

describe('LegalModal Unit Tests', () => {
  const mockOnClose = jest.fn();
  const mockOnAccept = jest.fn();
  const mockOnReject = jest.fn();

  const defaultProps = {
    visible: true,
    loading: false,
    title: 'Termeni și condiții',
    content: '**Acesta este un text important**',
    onClose: mockOnClose,
    onAccept: mockOnAccept,
    onReject: mockOnReject,
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('does not render its contents when visible is false', () => {
    const {queryByText} = render(
      <LegalModal {...defaultProps} visible={false} />,
    );
    expect(queryByText('Termeni și condiții')).toBeNull();
  });

  it('renders the title and loading spinner when loading is true', () => {
    render(<LegalModal {...defaultProps} loading={true} />);

    expect(screen.getByText('Termeni și condiții')).toBeTruthy();

    expect(screen.queryByTestId('mock-markdown')).toBeNull();
    expect(screen.queryByTestId('action-btn-Acceptă')).toBeNull();
    expect(screen.queryByTestId('action-btn-Refuză')).toBeNull();
  });

  it('renders markdown content and action buttons when loading is false', () => {
    render(<LegalModal {...defaultProps} loading={false} />);

    expect(screen.getByTestId('mock-markdown')).toBeTruthy();
    expect(screen.getByText('**Acesta este un text important**')).toBeTruthy();

    expect(screen.getByTestId('action-btn-Acceptă')).toBeTruthy();
    expect(screen.getByTestId('action-btn-Refuză')).toBeTruthy();
  });

  it('calls onClose when the close icon is pressed', () => {
    render(<LegalModal {...defaultProps} />);

    const closeBtn = screen.getByRole('button', {name: 'Închide'});
    fireEvent.press(closeBtn);

    expect(mockOnClose).toHaveBeenCalledTimes(1);
  });

  it('calls onAccept when the Accept button is pressed', () => {
    render(<LegalModal {...defaultProps} />);

    const acceptBtn = screen.getByTestId('action-btn-Acceptă');
    fireEvent.press(acceptBtn);

    expect(mockOnAccept).toHaveBeenCalledTimes(1);
  });

  it('calls onReject when the Reject button is pressed', () => {
    render(<LegalModal {...defaultProps} />);

    const rejectBtn = screen.getByTestId('action-btn-Refuză');
    fireEvent.press(rejectBtn);

    expect(mockOnReject).toHaveBeenCalledTimes(1);
  });
});
