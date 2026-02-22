import {API_URL} from '@/services/utils';
import {loginWithSocialThunk} from '@/store/auth/thunks';
import {useAppDispatch} from '@/store/hook';
import * as Linking from 'expo-linking';
import * as WebBrowser from 'expo-web-browser';
import {useCallback, useEffect} from 'react';
import {Alert, Platform} from 'react-native';

export function useSocialAuth() {
  const dispatch = useAppDispatch();

  const handleRedirectUrl = useCallback(
    (url: string) => {
      const parsed = Linking.parse(url);
      if (parsed.path === 'auth/callback') {
        const token = parsed.queryParams?.token as string;
        const refreshToken = parsed.queryParams?.refreshToken as string;
        if (token && refreshToken) {
          dispatch(loginWithSocialThunk({token, refreshToken}));
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
        'careerflowui://auth/callback',
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
        'careerflowui://auth/callback',
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
