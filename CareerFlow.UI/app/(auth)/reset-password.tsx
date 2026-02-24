import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/auth/GradientButton';
import {useRouter} from 'expo-router';
import React, {useState} from 'react';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {resetPasswordThunk} from '@/store/auth/thunks';

const ResetPasswordScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const [form, setForm] = useState({
    password: '',
    confirmPassword: '',
    email: '',
  });
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  const [touched, setTouched] = useState({
    password: false,
    confirmPassword: false,
    email: false,
  });

  const errors = {
    password: !form.password
      ? 'Parola este necesara'
      : form.password.length < 6
        ? 'Parola trebuie sa aiba minim 6 caractere'
        : null,
    confirmPassword: !form.confirmPassword
      ? 'Confirmarea parolei este necesara'
      : form.confirmPassword !== form.password
        ? 'Parolele nu se potrivesc'
        : null,
    email: !form.email
      ? 'Email-ul este necesar'
      : !emailRegex.test(form.email)
        ? 'Format invalid'
        : null,
  };

  const hasFieldErrors =
    !!errors.password || !!errors.confirmPassword || !!errors.email;
  const isFormValid = !hasFieldErrors;

  const handleResetPassword = async () => {
    if (!isFormValid || isLoading) return;

    try {
      await dispatch(
        resetPasswordThunk({email: form.email, newPassword: form.password}),
      ).unwrap();

      Toast.show({
        type: 'success',
        text1: 'Parolă resetată!',
        text2: 'Te poți autentifica cu noua parolă.',
      });

      setTimeout(() => router.replace('/(auth)/login'), 1500);
    } catch (error: unknown) {
      Toast.show({
        type: 'error',
        text1: 'Eroare la resetare',
        text2:
          typeof error === 'string'
            ? error
            : 'Link-ul poate fi expirat sau invalid.',
      });
    }
  };

  return (
    <AuthLayout
      title="Resetare Parolă"
      subtitle="Alege o nouă parolă securizată"
      footerText="Ți-ai amintit parola?"
      footerActionText="Înapoi la Login"
      onFooterAction={() => router.replace('/(auth)/login')}
      showTabs={false}
      showSocialAuth={false}
      showLegalLinks={false}>
      <AppInput
        label="Noua Parolă"
        icon="lock-outline"
        placeholder="Introdu noua parolă"
        isPassword
        value={form.password}
        onChangeText={text => setForm({...form, password: text})}
        onBlur={() => setTouched({...touched, password: true})}
        error={errors.password}
        touched={touched.password}
      />

      <AppInput
        label="Confirma Noua Parolă"
        icon="lock-outline"
        placeholder="Confirmă parola"
        isPassword
        value={form.confirmPassword}
        onChangeText={text => setForm({...form, confirmPassword: text})}
        onBlur={() => setTouched({...touched, confirmPassword: true})}
        error={errors.confirmPassword}
        touched={touched.confirmPassword}
      />

      <GradientButton
        text={isLoading ? 'Se procesează...' : 'Resetează Parola'}
        icon={isLoading ? null : 'lock-outline'}
        onPress={handleResetPassword}
        disabled={!isFormValid || isLoading}
      />
    </AuthLayout>
  );
};

export default ResetPasswordScreen;
