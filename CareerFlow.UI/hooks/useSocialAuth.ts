import {API_URL} from '@/services/utils';
import {loginWithSocialThunk} from '@/store/auth/thunks';
import {useAppDispatch} from '@/store/hook';
import * as Linking from 'expo-linking';
import {router} from 'expo-router';
import * as WebBrowser from 'expo-web-browser';
import {useCallback, useEffect} from 'react';
import {Alert, Platform} from 'react-native';

export function useSocialAuth() {
  const dispatch = useAppDispatch();

  const handleRedirectUrl = useCallback(
    async (url: string) => {
      // Verificăm robust dacă URL-ul conține calea noastră de callback
      if (url.includes('auth/callback')) {
        const parsed = Linking.parse(url);

        const token = parsed.queryParams?.token as string;
        const refreshToken = parsed.queryParams?.refreshToken as string;

        if (token && refreshToken) {
          try {
            // Așteptăm ca Redux să salveze sesiunea
            await dispatch(
              loginWithSocialThunk({token, refreshToken}),
            ).unwrap();

            // Închidem manual browserul pe iOS pentru a preveni blocajele vizuale
            if (Platform.OS === 'ios') {
              WebBrowser.dismissBrowser();
            }

            // Redirecționăm către ecranul de preferințe
            router.replace('/(auth)/preferences');
          } catch (error) {
            console.error('Eroare la social login dispatch:', error);
            Alert.alert('Eroare', 'Nu am putut finaliza autentificarea.');
          }
        } else {
          console.warn('Callback apelat, dar lipsesc tokenii din URL:', url);
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
        'careerflow://auth/callback', // Am eliminat 'ui'
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
        'careerflow://auth/callback', // Am eliminat 'ui'
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
