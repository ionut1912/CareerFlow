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
  const isProcessing = useRef(false);

  const handleRedirectUrl = useCallback(
    async (url: string) => {
      if (isProcessing.current) return;

      if (url.includes('auth/callback')) {
        isProcessing.current = true;
        isProcessing.current = true;

        try {
          const parsed = Linking.parse(url);
          if (parsed.queryParams?.error) {
            const errorType = parsed.queryParams.error;
            const serverMessage = parsed.queryParams.message as string;

            if (errorType === 'session_expired') {
              Alert.alert(
                'Sesiune Expirată',
                'Te rugăm să încerci să te autentifici din nou.',
              );
              return;
            }

            if (errorType === 'server_error' && serverMessage) {
              Alert.alert('Eroare Server', decodeURIComponent(serverMessage));
              return;
            }

            if (errorType === 'duplicate_request') {
              return;
            }
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
          }
        } catch {
          Alert.alert('Eroare', 'Nu am putut finaliza autentificarea.');
        } finally {
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
      const redirectUrl = Linking.createURL('auth/callback');
      const backendUrl = `${API_URL}/social/auth/google/mobile?returnUrl=${encodeURIComponent(redirectUrl)}`;

      const result = await WebBrowser.openAuthSessionAsync(
        backendUrl,
        redirectUrl,
      );

      if (result.type === 'success' && result.url) {
        handleRedirectUrl(result.url);
      }
    } catch {
      Alert.alert('Eroare', 'Autentificarea cu Google a eșuat.');
    }
  }, [handleRedirectUrl]);

  const loginWithLinkedin = useCallback(async () => {
    try {
      const redirectUrl = Linking.createURL('auth/callback');
      const backendUrl = `${API_URL}/social/auth/linkedin/mobile?returnUrl=${encodeURIComponent(redirectUrl)}`;

      const result = await WebBrowser.openAuthSessionAsync(
        backendUrl,
        redirectUrl,
      );

      if (result.type === 'success' && result.url) {
        handleRedirectUrl(result.url);
      }
    } catch {
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
