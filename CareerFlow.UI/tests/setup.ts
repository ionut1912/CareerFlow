import '@testing-library/jest-native/extend-expect';

jest.mock('react-native-reanimated', () => {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const Reanimated = require('react-native-reanimated/mock');
  Reanimated.default.call = () => {};
  return Reanimated;
});

jest.mock('expo-router', () => ({
  useRouter: () => ({push: jest.fn(), replace: jest.fn(), back: jest.fn()}),
  useLocalSearchParams: () => ({}),
}));
