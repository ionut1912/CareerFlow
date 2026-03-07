import 'react-native-gesture-handler/jestSetup';

// Mock expo-router
jest.mock('expo-router', () => ({
  useRouter: () => ({push: jest.fn(), replace: jest.fn(), back: jest.fn()}),
  useLocalSearchParams: () => ({}),
  Link: 'Link',
}));

// Silence noisy native module warnings
jest.mock('@react-native-google-signin/google-signin', () => ({}));
