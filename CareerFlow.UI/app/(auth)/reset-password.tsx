import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/shared/GradientButton';
import {resetPasswordThunk} from '@/store/auth/thunks';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {useLocalSearchParams, useRouter} from 'expo-router';
import React from 'react';
import {useFormState} from '@/hooks/useFormState';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {
  validateEmail,
  validatePassword,
  validateConfirmPassword,
} from '@/utils/validators';

const INITIAL_FORM = {email: '', password: '', confirmPassword: ''};

const ResetPasswordScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {token} = useLocalSearchParams<{token: string}>();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const {form, touched, handleChange, handleBlur} = useFormState(INITIAL_FORM);

  const errors = {
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    confirmPassword: validateConfirmPassword(
      form.password,
      form.confirmPassword,
    ),
  };

  const isFormValid = !Object.values(errors).some(Boolean);

  const handleResetPassword = async () => {
    if (!isFormValid || isLoading) return;
    try {
      await dispatch(
        resetPasswordThunk({
          email: form.email,
          newPassword: form.password,
          token,
        }),
      ).unwrap();
      showSuccessToast('Parolă resetată!');
      setTimeout(() => router.replace('/(auth)/login'), 1500);
    } catch (err) {
      showErrorToast('Eroare la resetare', err);
    }
  };

  return (
    <AuthLayout
      title="Resetare Parolă"
      subtitle=""
      showTabs={false}
      showSocialAuth={false}
      showLegalLinks={false}>
      <AppInput
        label="Email"
        icon="mail-outline"
        placeholder="you@example.com"
        keyboardType="email-address"
        value={form.email}
        onChangeText={handleChange('email')}
        onBlur={handleBlur('email')}
        error={errors.email}
        touched={touched.email}
      />
      <AppInput
        label="Noua Parolă"
        icon="lock-outline"
        isPassword
        placeholder="••••••••"
        value={form.password}
        onChangeText={handleChange('password')}
        onBlur={handleBlur('password')}
        error={errors.password}
        touched={touched.password}
      />
      <AppInput
        label="Confirmă Parola"
        icon="lock-outline"
        isPassword
        placeholder="••••••••"
        value={form.confirmPassword}
        onChangeText={handleChange('confirmPassword')}
        onBlur={handleBlur('confirmPassword')}
        error={errors.confirmPassword}
        touched={touched.confirmPassword}
      />
      <GradientButton
        text={isLoading ? 'Se procesează...' : 'Resetează Parola'}
        onPress={handleResetPassword}
        disabled={!isFormValid || isLoading}
      />
    </AuthLayout>
  );
};

export default ResetPasswordScreen;
