import {renderHook, act} from '@testing-library/react-native';
import {Platform, Alert} from 'react-native';
import * as WebBrowser from 'expo-web-browser';
import * as Linking from 'expo-linking';
import {useAppDispatch} from '@/store/hook';
import {loginWithSocialThunk} from '@/store/auth/thunks';
import {useSocialAuth} from '@/hooks/useSocialAuth';

jest.mock('@/store/hook', () => ({
  useAppDispatch: jest.fn(),
}));

jest.mock('@/store/auth/thunks', () => ({
  loginWithSocialThunk: jest.fn(),
}));

jest.mock('@/services/utils', () => ({
  API_URL: 'https://mock-api.com',
}));

jest.mock('expo-web-browser', () => ({
  openAuthSessionAsync: jest.fn(),
}));

const mockRemoveListener = jest.fn();
jest.mock('expo-linking', () => ({
  parse: jest.fn(),
  addEventListener: jest.fn(() => ({remove: mockRemoveListener})),
}));

describe('useSocialAuth', () => {
  const mockDispatch = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useAppDispatch as jest.Mock).mockReturnValue(mockDispatch);
    jest.spyOn(console, 'error').mockImplementation(() => {});
    jest.spyOn(Alert, 'alert').mockImplementation(() => {});
    Platform.OS = 'ios';
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('initializes with social providers ready', () => {
    const {result} = renderHook(() => useSocialAuth());
    expect(result.current.googleReady).toBe(true);
    expect(result.current.linkedinReady).toBe(true);
  });

  describe('Authentication Flows', () => {
    it('handles successful Google login and dispatches tokens', async () => {
      const mockRedirectUrl =
        'careerflowui://auth/callback?token=123&refreshToken=abc';
      (WebBrowser.openAuthSessionAsync as jest.Mock).mockResolvedValueOnce({
        type: 'success',
        url: mockRedirectUrl,
      });

      (Linking.parse as jest.Mock).mockReturnValueOnce({
        path: 'auth/callback',
        queryParams: {token: '123', refreshToken: 'abc'},
      });

      const {result} = renderHook(() => useSocialAuth());

      await act(async () => {
        await result.current.loginWithGoogle();
      });

      expect(WebBrowser.openAuthSessionAsync).toHaveBeenCalledWith(
        'https://mock-api.com/social/auth/google/mobile',
        'careerflowui://auth/callback',
      );
      expect(Linking.parse).toHaveBeenCalledWith(mockRedirectUrl);
      expect(mockDispatch).toHaveBeenCalledWith(
        loginWithSocialThunk({token: '123', refreshToken: 'abc'}),
      );
    });

    it('handles successful LinkedIn login and dispatches tokens', async () => {
      const mockRedirectUrl =
        'careerflowui://auth/callback?token=456&refreshToken=def';
      (WebBrowser.openAuthSessionAsync as jest.Mock).mockResolvedValueOnce({
        type: 'success',
        url: mockRedirectUrl,
      });

      (Linking.parse as jest.Mock).mockReturnValueOnce({
        path: 'auth/callback',
        queryParams: {token: '456', refreshToken: 'def'},
      });

      const {result} = renderHook(() => useSocialAuth());

      await act(async () => {
        await result.current.loginWithLinkedin();
      });

      expect(WebBrowser.openAuthSessionAsync).toHaveBeenCalledWith(
        'https://mock-api.com/social/auth/linkedin/mobile',
        'careerflowui://auth/callback',
      );
      expect(mockDispatch).toHaveBeenCalledTimes(1);
    });

    it('does not dispatch if the browser session is cancelled or dismissed', async () => {
      (WebBrowser.openAuthSessionAsync as jest.Mock).mockResolvedValueOnce({
        type: 'cancel',
      });

      const {result} = renderHook(() => useSocialAuth());

      await act(async () => {
        await result.current.loginWithGoogle();
      });

      expect(Linking.parse).not.toHaveBeenCalled();
      expect(mockDispatch).not.toHaveBeenCalled();
    });

    it('handles Google login errors and shows an alert', async () => {
      (WebBrowser.openAuthSessionAsync as jest.Mock).mockRejectedValueOnce(
        new Error('Browser crashed'),
      );

      const {result} = renderHook(() => useSocialAuth());

      await act(async () => {
        await result.current.loginWithGoogle();
      });

      expect(Alert.alert).toHaveBeenCalledWith(
        'Eroare',
        'Autentificarea cu Google a eșuat.',
      );
      expect(mockDispatch).not.toHaveBeenCalled();
    });
  });

  describe('Platform Specific Behavior (Android Deep Linking)', () => {
    it('attaches and cleans up a Linking event listener only on Android', () => {
      Platform.OS = 'android';

      const {unmount} = renderHook(() => useSocialAuth());

      expect(Linking.addEventListener).toHaveBeenCalledWith(
        'url',
        expect.any(Function),
      );

      unmount();
      expect(mockRemoveListener).toHaveBeenCalledTimes(1);
    });

    it('does NOT attach a Linking event listener on iOS', () => {
      Platform.OS = 'ios';

      renderHook(() => useSocialAuth());

      expect(Linking.addEventListener).not.toHaveBeenCalled();
    });

    it('processes incoming Android deep links via the event listener', () => {
      Platform.OS = 'android';

      let registeredCallback: (event: {url: string}) => void = () => {};
      (Linking.addEventListener as jest.Mock).mockImplementationOnce(
        (event, cb) => {
          registeredCallback = cb;
          return {remove: mockRemoveListener};
        },
      );

      (Linking.parse as jest.Mock).mockReturnValueOnce({
        path: 'auth/callback',
        queryParams: {token: 'android-token', refreshToken: 'android-refresh'},
      });

      renderHook(() => useSocialAuth());

      act(() => {
        registeredCallback({
          url: 'careerflowui://auth/callback?token=android-token&refreshToken=android-refresh',
        });
      });

      expect(mockDispatch).toHaveBeenCalledWith(
        loginWithSocialThunk({
          token: 'android-token',
          refreshToken: 'android-refresh',
        }),
      );
    });
  });
});
