import {
  loginWithGoogleThunk,
  loginWithLinkedinThunk,
} from '@/store/auth/thunks';
import {useAppDispatch} from '@/store/hook';
import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import * as Google from 'expo-auth-session/providers/google';
import {useCallback} from 'react';
import {Alert} from 'react-native';

WebBrowser.maybeCompleteAuthSession();

export function useSocialAuth() {
  const dispatch = useAppDispatch();

  const redirectUri = AuthSession.makeRedirectUri({
    scheme: 'careerflowui',
    useProxy: true,
  });

  const [googleRequest, , promptGoogleAsync] = Google.useAuthRequest({
    webClientId:
      '576337091837-7ovoom2s6a6rgb96054eocjn6pul5umd.apps.googleusercontent.com',
    iosClientId:
      '576337091837-0h5gevhujklokgrlh2csibkqbnh0v23v.apps.googleusercontent.com',
    androidClientId:
      '576337091837-h53851l17k77t8tf398akcs6pkf1ds3c.apps.googleusercontent.com',
    redirectUri,
  });

  const loginWithGoogle = useCallback(async () => {
    try {
      const result = await promptGoogleAsync();
      if (result?.type === 'success') {
        const {id_token} = result.params;
        if (!id_token) throw new Error('No idToken');
        await dispatch(loginWithGoogleThunk(id_token)).unwrap();
      }
    } catch (error: unknown) {
      console.error(error);
      Alert.alert('Eroare', 'Autentificarea cu Google a eșuat.');
    }
  }, [promptGoogleAsync, dispatch]);

  const [linkedinRequest, , promptLinkedinAsync] = AuthSession.useAuthRequest(
    {
      clientId: '778j163stk31vq',
      scopes: ['openid', 'profile', 'email'],
      redirectUri: AuthSession.makeRedirectUri({
        scheme: 'careerflowui',
        useProxy: true,
      }),
    },
    {
      authorizationEndpoint: 'https://www.linkedin.com/oauth/v2/authorization',
      tokenEndpoint: 'https://www.linkedin.com/oauth/v2/accessToken',
    },
  );

  const loginWithLinkedin = useCallback(async () => {
    try {
      const result = await promptLinkedinAsync();
      if (result?.type === 'success') {
        await dispatch(loginWithLinkedinThunk(result.params.code)).unwrap();
      }
    } catch {
      Alert.alert('Eroare', 'Autentificarea cu LinkedIn a eșuat.');
    }
  }, [promptLinkedinAsync, dispatch]);

  return {
    loginWithGoogle,
    loginWithLinkedin,
    googleReady: !!googleRequest,
    linkedinReady: !!linkedinRequest,
  };
}
