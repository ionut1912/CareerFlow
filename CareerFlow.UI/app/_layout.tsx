import {useColorScheme} from '@/hooks/use-color-scheme';
import {useAppDispatch} from '@/store/hook';
import {restoreSessionThunk} from '@/store/auth/thunks';
import {store} from '@/store/store';
import {DarkTheme, DefaultTheme, ThemeProvider} from '@react-navigation/native';
import {Stack} from 'expo-router';
import {StatusBar} from 'expo-status-bar';
import React, {useEffect} from 'react';
import 'react-native-reanimated';
import Toast from 'react-native-toast-message';
import {Provider} from 'react-redux';

export const unstable_settings = {
  initialRouteName: '(auth)/login',
};

/** Shared screen option — avoids repeating { headerShown: false } (DRY). */
const HIDDEN_HEADER = {headerShown: false} as const;

export default function RootLayout() {
  return (
    <Provider store={store}>
      <AppLayout />
    </Provider>
  );
}

/**
 * Inner layout component — separated so it can access the Redux store
 * via hooks (Provider must be an ancestor).
 */
function AppLayout() {
  const colorScheme = useColorScheme();
  const dispatch = useAppDispatch();

  useEffect(() => {
    dispatch(restoreSessionThunk());
  }, [dispatch]);

  return (
    <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
      <Stack screenOptions={HIDDEN_HEADER}>
        <Stack.Screen name="(auth)" />
        <Stack.Screen name="(tabs)" />
        <Stack.Screen name="index" />
        <Stack.Screen
          name="modal"
          options={{presentation: 'modal', title: 'Modal', headerShown: true}}
        />
      </Stack>
      <StatusBar style="auto" />
      <Toast />
    </ThemeProvider>
  );
}
