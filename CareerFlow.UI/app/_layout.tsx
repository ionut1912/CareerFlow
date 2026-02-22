import {useColorScheme} from '@/hooks/use-color-scheme';
import {DarkTheme, DefaultTheme, ThemeProvider} from '@react-navigation/native';
import {Stack} from 'expo-router';
import {StatusBar} from 'expo-status-bar';
import Toast from 'react-native-toast-message';
import 'react-native-reanimated';
import React from 'react';
import {Provider} from 'react-redux';
import {store} from '@/store/store';

export const unstable_settings = {
  initialRouteName: '(auth)/login',
};

export default function RootLayout() {
  const colorScheme = useColorScheme();

  return (
    <Provider store={store}>
      <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
        <Stack screenOptions={{headerShown: false}}>
          <Stack.Screen name="(auth)" options={{headerShown: false}} />
          <Stack.Screen name="(tabs)" options={{headerShown: false}} />
          <Stack.Screen name="index" options={{headerShown: false}} />
          <Stack.Screen
            name="modal"
            options={{presentation: 'modal', title: 'Modal', headerShown: true}}
          />
        </Stack>
        <StatusBar style="auto" />
        <Toast />
      </ThemeProvider>
    </Provider>
  );
}
