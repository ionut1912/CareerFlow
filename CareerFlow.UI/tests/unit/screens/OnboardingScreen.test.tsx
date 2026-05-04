import React from 'react';
import {render, fireEvent, waitFor} from '@testing-library/react-native';
import {Dimensions} from 'react-native';
import {useRouter} from 'expo-router';
import {useAppDispatch} from '@/store/hook';
import {completeOnboardingThunk} from '@/store/app/thunks';
import OnboardingScreen from '@/app/(onboarding)';

jest.mock('@/store/hook', () => ({
  useAppDispatch: jest.fn(),
}));

jest.mock('@/store/app/thunks', () => ({
  completeOnboardingThunk: jest.fn(() => ({
    unwrap: jest.fn().mockResolvedValue(true),
  })),
}));

jest.mock('@/constants/onboardingData', () => ({
  ONBOARDING_STEPS: [
    {
      title: 'Step 1 Title',
      subtitle: 'Step 1 Subtitle',
      Illustration: () => null,
    },
    {
      title: 'Step 2 Title',
      subtitle: 'Step 2 Subtitle',
      Illustration: () => null,
    },
    {
      title: 'Step 3 Title',
      subtitle: 'Step 3 Subtitle',
      Illustration: () => null,
    },
  ],
}));

jest.mock('react-native-safe-area-context', () => ({
  SafeAreaView: ({children}: {children: React.ReactNode}) => children,
}));

interface GradientButtonProps {
  text: string;
  onPress: () => void;
}

jest.mock('@/components/shared/GradientButton', () => ({
  GradientButton: ({text, onPress}: GradientButtonProps) => {
    const MockButton = 'MockButton' as unknown as React.ElementType;
    return (
      <MockButton testID="get-started-button" onPress={onPress}>
        {text}
      </MockButton>
    );
  },
}));

jest.mock('@/components/onboarding/ProgressDots', () => ({
  ProgressDots: () => {
    const MockProgress = 'MockProgress' as unknown as React.ElementType;
    return <MockProgress testID="progress-dots" />;
  },
}));

describe('OnboardingScreen Integration', () => {
  const {width} = Dimensions.get('window');
  const mockReplace = jest.fn();
  const mockDispatch = jest.fn(action => action);

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({replace: mockReplace});
    (useAppDispatch as jest.Mock).mockReturnValue(mockDispatch);
  });

  it('renders the first slide correctly on mount', () => {
    const {getByText, queryByTestId} = render(<OnboardingScreen />);
    expect(getByText('Step 1 Title')).toBeTruthy();
    expect(getByText('Step 1 Subtitle')).toBeTruthy();
    expect(getByText('Gliseaza pentru a continua')).toBeTruthy();
    expect(queryByTestId('get-started-button')).toBeNull();
  });

  it('updates the visible step text when scrolling', () => {
    const {getByText, getByTestId} = render(<OnboardingScreen />);
    const flatList = getByTestId('onboarding-list');

    fireEvent.scroll(flatList, {
      nativeEvent: {
        contentOffset: {x: width * 1, y: 0},
        layoutMeasurement: {width, height: 100},
        contentSize: {width: width * 3, height: 100},
      },
    });

    expect(getByText('Step 2 Title')).toBeTruthy();
    expect(getByText('Step 2 Subtitle')).toBeTruthy();
    expect(getByText('Gliseaza pentru a continua')).toBeTruthy();
  });

  it('reveals the "Get Started" button upon reaching the final slide', () => {
    const {getByText, getByTestId, queryByText} = render(<OnboardingScreen />);
    const flatList = getByTestId('onboarding-list');

    fireEvent.scroll(flatList, {
      nativeEvent: {
        contentOffset: {x: width * 2, y: 0},
        layoutMeasurement: {width, height: 100},
        contentSize: {width: width * 3, height: 100},
      },
    });

    expect(getByText('Step 3 Title')).toBeTruthy();
    expect(queryByText('Gliseaza pentru a continua')).toBeNull();
    expect(getByTestId('get-started-button')).toBeTruthy();
  });

  it('dispatches complete thunk and redirects to login on final button press', async () => {
    const {getByTestId} = render(<OnboardingScreen />);
    const flatList = getByTestId('onboarding-list');

    fireEvent.scroll(flatList, {
      nativeEvent: {
        contentOffset: {x: width * 2, y: 0},
        layoutMeasurement: {width, height: 100},
        contentSize: {width: width * 3, height: 100},
      },
    });

    const startButton = getByTestId('get-started-button');
    fireEvent.press(startButton);

    expect(completeOnboardingThunk).toHaveBeenCalled();
    expect(mockDispatch).toHaveBeenCalled();

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith('/(auth)/login');
    });
  });
});
