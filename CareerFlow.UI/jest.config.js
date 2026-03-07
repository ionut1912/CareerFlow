/** @type {import('jest').Config} */
module.exports = {
  preset: 'jest-expo',
  setupFilesAfterEnv: [
    '@testing-library/jest-native/extend-expect',
    '<rootDir>/jest.setup.ts',
  ],
  transformIgnorePatterns: [
    'node_modules/(?!((jest-)?react-native|@react-native(-community)?|expo(nent)?|@expo(nent)?/.*|@expo-google-fonts/.*|react-navigation|@react-navigation/.*|nativewind|tailwind-merge|lucide-react-native|react-native-reanimated|react-redux|@reduxjs/toolkit|immer))',
  ],
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
  clearMocks: true,
};
