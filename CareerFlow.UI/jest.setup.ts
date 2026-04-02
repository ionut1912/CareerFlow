import 'react-native-gesture-handler/jestSetup';
import React from 'react';
import {Text} from 'react-native';

jest.mock('axios', () => {
  const mockAxios = {
    create: jest.fn(() => mockAxios),
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
    interceptors: {
      request: {use: jest.fn(), eject: jest.fn()},
      response: {use: jest.fn(), eject: jest.fn()},
    },
    defaults: {
      headers: {common: {}},
      adapter: 'http',
    },
  };
  return mockAxios;
});

const mockReact = React;
const mockText = Text;

jest.mock('expo-router', () => ({
  useRouter: jest.fn(() => ({
    push: jest.fn(),
    replace: jest.fn(),
    back: jest.fn(),
  })),
  useLocalSearchParams: jest.fn(() => ({})),
  usePathname: jest.fn(() => '/'),
  Link: ({children}: {children: React.ReactNode}) =>
    mockReact.createElement(mockText, {}, children),
}));

jest.mock('@react-native-google-signin/google-signin', () => ({
  GoogleSignin: {
    configure: jest.fn(),
    hasPlayServices: jest.fn(),
    signIn: jest.fn(),
    signOut: jest.fn(),
  },
}));

jest.mock('@expo/vector-icons', () => {
  const MockIcon = ({name}: {name: string}) =>
    mockReact.createElement(mockText, {}, name);
  return {
    Ionicons: MockIcon,
    MaterialIcons: MockIcon,
    MaterialCommunityIcons: MockIcon,
    FontAwesome: MockIcon,
    createIconSet: () => MockIcon,
  };
});
