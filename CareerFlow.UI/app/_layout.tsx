import {useColorScheme} from '@/hooks/use-color-scheme';
import {useAppDispatch} from '@/store/hook';
import {restoreSessionThunk} from '@/store/auth/thunks';
import {store} from '@/store/store';
import {DarkTheme, DefaultTheme, ThemeProvider} from '@react-navigation/native';
import {Stack} from 'expo-router';
import {StatusBar} from 'expo-status-bar';
import React, {useEffect, useState} from 'react';
import 'react-native-reanimated';
import Toast from 'react-native-toast-message';
import {Provider} from 'react-redux';
import {initializeAppStatusThunk} from '@/store/app/thunks';
import * as SplashScreen from 'expo-splash-screen';

export const unstable_settings = {initialRouteName: '(auth)/login'};
const HIDDEN_HEADER = {headerShown: false} as const;

// 1. Tell the splash screen to stay visible while we load our data
SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  return (
    <Provider store={store}>
      <AppLayout />
    </Provider>
  );
}

function AppLayout() {
  const colorScheme = useColorScheme();
  const dispatch = useAppDispatch();
  const [isAppReady, setIsAppReady] = useState(false);

  useEffect(() => {
    async function prepareApp() {
      try {
        // 2. Wait for both initialization thunks to finish
        // .unwrap() ensures we catch any errors thrown by the thunks
        await Promise.all([
          dispatch(restoreSessionThunk()).unwrap(),
          dispatch(initializeAppStatusThunk()).unwrap(),
        ]);
      } catch (e) {
        console.warn('App initialization error:', e);
      } finally {
        // 3. Mark the app as ready and hide the splash screen
        setIsAppReady(true);
        await SplashScreen.hideAsync();
      }
    }

    prepareApp();
  }, [dispatch]);

  // 4. Return nothing (or a blank view) while the splash screen is still showing
  if (!isAppReady) {
    return null;
  }

  return (
    <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
      <Stack screenOptions={HIDDEN_HEADER}>
        <Stack.Screen name="(onboarding)" />
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
