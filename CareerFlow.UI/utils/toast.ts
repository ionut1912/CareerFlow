import Toast from 'react-native-toast-message';

export const showSuccessToast = (text1: string, text2?: string) =>
  Toast.show({type: 'success', text1, text2});

export const showErrorToast = (
  text1: string,
  error: unknown,
  fallback = 'Ceva nu a functionat corect.',
) =>
  Toast.show({
    type: 'error',
    text1,
    text2: typeof error === 'string' ? error : fallback,
  });
