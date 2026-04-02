import React from 'react';
import {render, fireEvent, waitFor} from '@testing-library/react-native';
import {TouchableOpacity, Text, View as MockView} from 'react-native';
import {useRouter} from 'expo-router';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {createUserProfileThunk} from '@/store/userProfile/thunks';
import {showSuccessToast, showErrorToast} from '@/utils/toast';
import PreferencesScreen from '@/app/(auth)/preferences';

const MockTouchableOpacity = TouchableOpacity;
const MockText = Text;

jest.mock('@/store/hook', () => ({
  useAppDispatch: jest.fn(),
  useAppSelector: jest.fn(),
}));

jest.mock('@/store/userProfile/thunks', () => ({
  createUserProfileThunk: jest.fn(),
}));

jest.mock('@/utils/toast', () => ({
  showSuccessToast: jest.fn(),
  showErrorToast: jest.fn(),
}));

interface MockFlashListProps<T> {
  data?: T[];
  renderItem: (info: {item: T; index: number}) => React.ReactNode;
  ListHeaderComponent?: React.ReactNode;
}

jest.mock('@shopify/flash-list', () => ({
  FlashList: <T extends {id?: string}>({
    data,
    renderItem,
    ListHeaderComponent,
  }: MockFlashListProps<T>) => (
    <MockView>
      {ListHeaderComponent}
      {data?.map((item, index) => (
        <MockView key={item?.id || index}>{renderItem({item, index})}</MockView>
      ))}
    </MockView>
  ),
}));

jest.mock('@/components/shared/OptionCard', () => {
  return {
    OptionCard: ({
      item,
      onPress,
    }: {
      item: {id: string; title: string};
      onPress: () => void;
    }) => (
      <MockTouchableOpacity onPress={onPress} testID={`option-${item.id}`}>
        <MockText>{item.title}</MockText>
      </MockTouchableOpacity>
    ),
  };
});

jest.mock('@/components/shared/GradientButton', () => {
  return {
    GradientButton: ({
      text,
      onPress,
      disabled,
    }: {
      text: string;
      onPress: () => void;
      disabled?: boolean;
    }) => (
      <MockTouchableOpacity
        onPress={disabled ? undefined : onPress}
        disabled={disabled}
        testID="gradient-button"
        accessibilityState={{disabled}}>
        <MockText>{text}</MockText>
      </MockTouchableOpacity>
    ),
  };
});

describe('PreferencesScreen', () => {
  const mockRouter = {replace: jest.fn()};
  const mockDispatch = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue(mockRouter);
    (useAppDispatch as jest.Mock).mockReturnValue(mockDispatch);
    (useAppSelector as unknown as jest.Mock).mockReturnValue({loading: false});
  });

  it('renders Step 1 correctly and keeps "Continuă" disabled initially', () => {
    const {getByText, getByTestId} = render(<PreferencesScreen />);
    expect(
      getByText('Care este obiectivul tău?', {includeHiddenElements: true}),
    ).toBeTruthy();
    expect(
      getByText('Pasul 1 din 2', {includeHiddenElements: true}),
    ).toBeTruthy();

    const nextButton = getByTestId('gradient-button');
    expect(nextButton.props.accessibilityState.disabled).toBe(true);
  });

  it('allows selecting motivations and advancing to Step 2', () => {
    const {getByText, getByTestId} = render(<PreferencesScreen />);
    const studentOption = getByTestId('option-Student');
    fireEvent.press(studentOption);

    const nextButton = getByTestId('gradient-button');
    expect(nextButton.props.accessibilityState.disabled).toBe(false);

    fireEvent.press(nextButton);
    expect(
      getByText('Cum preferi să înveți?', {includeHiddenElements: true}),
    ).toBeTruthy();
    expect(
      getByText('Pasul 2 din 2', {includeHiddenElements: true}),
    ).toBeTruthy();
  });

  it('allows going back from Step 2 to Step 1', () => {
    const {getByText, getByTestId} = render(<PreferencesScreen />);
    fireEvent.press(getByTestId('option-Student'));
    fireEvent.press(getByTestId('gradient-button'));

    const backButton = getByText('Înapoi', {includeHiddenElements: true});
    fireEvent.press(backButton);
    expect(
      getByText('Care este obiectivul tău?', {includeHiddenElements: true}),
    ).toBeTruthy();
  });

  it('submits the form successfully, dispatches thunk, shows toast, and navigates', async () => {
    mockDispatch.mockReturnValueOnce({
      unwrap: jest.fn().mockResolvedValue(true),
    });

    const {getByTestId} = render(<PreferencesScreen />);
    fireEvent.press(getByTestId('option-Student'));
    fireEvent.press(getByTestId('gradient-button'));
    fireEvent.press(getByTestId('option-Visual'));
    fireEvent.press(getByTestId('gradient-button'));

    await waitFor(() => {
      expect(createUserProfileThunk).toHaveBeenCalledWith({
        learningType: 'Visual',
        userTypes: ['Student'],
      });
      expect(mockDispatch).toHaveBeenCalledTimes(1);
      expect(showSuccessToast).toHaveBeenCalledWith(
        'Preferințe salvate!',
        'Bucură-te de învățare!',
      );
      expect(mockRouter.replace).toHaveBeenCalledWith('/(tabs)');
    });
  });

  it('shows an error toast if submission fails', async () => {
    const mockError = new Error('Network error');
    mockDispatch.mockReturnValueOnce({
      unwrap: jest.fn().mockRejectedValue(mockError),
    });

    const {getByTestId} = render(<PreferencesScreen />);
    fireEvent.press(getByTestId('option-Student'));
    fireEvent.press(getByTestId('gradient-button'));
    fireEvent.press(getByTestId('option-Visual'));
    fireEvent.press(getByTestId('gradient-button'));

    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Eroare la salvarea preferințelor',
        mockError,
      );
      expect(mockRouter.replace).not.toHaveBeenCalled();
    });
  });
});
