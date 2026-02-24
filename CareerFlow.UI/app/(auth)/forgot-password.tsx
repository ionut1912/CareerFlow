import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/auth/GradientButton';
import {useRouter} from 'expo-router';
import React, {useState} from 'react';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {requestPasswordResetThunk} from '@/store/auth/thunks';

const ForgotPasswordScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const [email, setEmail] = useState('');
  const [touched, setTouched] = useState(false);

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const error = !email
    ? 'Email-ul este necesar'
    : !emailRegex.test(email)
      ? 'Format invalid'
      : null;

  const isFormValid = !error;

  const handleRequestReset = async () => {
    if (!isFormValid || isLoading) return;

    try {
      await dispatch(requestPasswordResetThunk({email})).unwrap();

      Toast.show({
        type: 'success',
        text1: 'Email trimis!',
        text2: 'Verifică-ți căsuța de email pentru link-ul de resetare.',
      });

      setTimeout(() => router.replace('/(auth)/login'), 2000);
    } catch (error: unknown) {
      Toast.show({
        type: 'error',
        text1: 'Eroare',
        text2:
          typeof error === 'string' ? error : 'Nu am putut trimite email-ul.',
      });
    }
  };

  return (
    <AuthLayout
      title="Ai uitat parola?"
      subtitle="Introdu email-ul pentru a o reseta"
      footerText="Ți-ai amintit parola?"
      footerActionText="Înapoi la Login"
      onFooterAction={() => router.replace('/(auth)/login')}
      showTabs={false}
      showSocialAuth={false}
      showLegalLinks={false}>
      <AppInput
        label="Adresa de email"
        icon="mail-outline"
        placeholder="you@example.com"
        keyboardType="email-address"
        value={email}
        onChangeText={setEmail}
        onBlur={() => setTouched(true)}
        error={error}
        touched={touched}
      />

      <GradientButton
        text={isLoading ? 'Se trimite...' : 'Trimite link de resetare'}
        icon={isLoading ? null : 'send'}
        onPress={handleRequestReset}
        disabled={!isFormValid || isLoading}
      />
    </AuthLayout>
  );
};

export default ForgotPasswordScreen;
