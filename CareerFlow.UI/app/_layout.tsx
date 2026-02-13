import {useColorScheme} from '@/hooks/use-color-scheme';
import {DarkTheme, DefaultTheme, ThemeProvider} from '@react-navigation/native';
import {Stack} from 'expo-router';
import {StatusBar} from 'expo-status-bar';
import 'react-native-reanimated';

export const unstable_settings = {
  // Ensure the app starts at the login screen
  initialRouteName: '(auth)/login',
};

export default function RootLayout() {
  const colorScheme = useColorScheme();

  return (
    <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
      <Stack screenOptions={{headerShown: false}}>
        {/* Auth Routes */}
        <Stack.Screen name="(auth)/login" options={{headerShown: false}} />
        <Stack.Screen name="(auth)/register" options={{headerShown: false}} />

        {/* Main App Routes */}
        <Stack.Screen name="(tabs)" options={{headerShown: false}} />

        {/* Root Index (Redirect logic usually goes here) */}
        <Stack.Screen name="index" options={{headerShown: false}} />

        {/* Modals */}
        <Stack.Screen
          name="modal"
          options={{presentation: 'modal', title: 'Modal', headerShown: true}}
        />
      </Stack>
      <StatusBar style="auto" />
    </ThemeProvider>
  );
}
