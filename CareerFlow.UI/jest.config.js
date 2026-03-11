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

  // ADD THIS BLOCK:
  moduleNameMapper: {
    '^@react-native-async-storage/async-storage$':
      '<rootDir>/node_modules/@react-native-async-storage/async-storage/jest/async-storage-mock',
  },
};
