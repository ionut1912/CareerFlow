import {API_URL} from '@/services/utils';
import {loginWithSocialThunk} from '@/store/auth/thunks';
import {useAppDispatch} from '@/store/hook';
import * as Linking from 'expo-linking';
import {router} from 'expo-router';
import * as WebBrowser from 'expo-web-browser';
import {useCallback, useEffect, useRef} from 'react';
import {Alert, Platform} from 'react-native';

export function useSocialAuth() {
  const dispatch = useAppDispatch();

  // Fix: Add a lock to prevent the app from processing the redirect twice simultaneously
  const isProcessing = useRef(false);

  const handleRedirectUrl = useCallback(
    async (url: string) => {
      // Exit immediately if we are already handling a callback
      if (isProcessing.current) return;

      if (url.includes('auth/callback')) {
        isProcessing.current = true; // Lock

        try {
          const parsed = Linking.parse(url);

          // Fix: Check if the backend sent us an error instead of a token
          if (parsed.queryParams?.error) {
            Alert.alert(
              'Sesiune Expirată',
              'Te rugăm să încerci să te autentifici din nou.',
            );
            return;
          }

          const token = parsed.queryParams?.token as string;
          const refreshToken = parsed.queryParams?.refreshToken as string;

          if (token && refreshToken) {
            await dispatch(
              loginWithSocialThunk({token, refreshToken}),
            ).unwrap();

            if (Platform.OS === 'ios') {
              WebBrowser.dismissBrowser();
            }

            router.replace('/(auth)/preferences');
          } else {
            console.warn('Callback apelat, dar lipsesc tokenii din URL:', url);
          }
        } catch (error) {
          console.error('Eroare la social login dispatch:', error);
          Alert.alert('Eroare', 'Nu am putut finaliza autentificarea.');
        } finally {
          // Always release the lock when done
          isProcessing.current = false;
        }
      }
    },
    [dispatch],
  );

  useEffect(() => {
    if (Platform.OS === 'android') {
      const subscription = Linking.addEventListener('url', ({url}) => {
        handleRedirectUrl(url);
      });
      return () => subscription.remove();
    }
  }, [handleRedirectUrl]);

  const loginWithGoogle = useCallback(async () => {
    try {
      const result = await WebBrowser.openAuthSessionAsync(
        `${API_URL}/social/auth/google/mobile`,
        'careerflow://auth/callback',
      );
      if (result.type === 'success' && result.url) {
        handleRedirectUrl(result.url);
      }
    } catch (error: unknown) {
      console.error(error);
      Alert.alert('Eroare', 'Autentificarea cu Google a eșuat.');
    }
  }, [handleRedirectUrl]);

  const loginWithLinkedin = useCallback(async () => {
    try {
      const result = await WebBrowser.openAuthSessionAsync(
        `${API_URL}/social/auth/linkedin/mobile`,
        'careerflow://auth/callback',
      );
      if (result.type === 'success' && result.url) {
        handleRedirectUrl(result.url);
      }
    } catch (error: unknown) {
      console.error(error);
      Alert.alert('Eroare', 'Autentificarea cu LinkedIn a eșuat.');
    }
  }, [handleRedirectUrl]);

  return {
    loginWithGoogle,
    loginWithLinkedin,
    googleReady: true,
    linkedinReady: true,
  };
}
