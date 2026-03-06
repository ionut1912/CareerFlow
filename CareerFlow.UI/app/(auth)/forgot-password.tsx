import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/shared/GradientButton';
import {requestPasswordResetThunk} from '@/store/auth/thunks';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {useRouter} from 'expo-router';
import React from 'react';
import {useFormState} from '@/hooks/useFormState';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {validateEmail} from '@/utils/validators';

const ForgotPasswordScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const {form, touched, handleChange, handleBlur} = useFormState({email: ''});

  const error = validateEmail(form.email);
  const isFormValid = !error;

  const handleRequestReset = async () => {
    if (!isFormValid || isLoading) return;
    try {
      await dispatch(requestPasswordResetThunk({email: form.email})).unwrap();
      showSuccessToast(
        'Email trimis!',
        'Verifică-ți căsuța de email pentru link-ul de resetare.',
      );
      setTimeout(() => router.replace('/(auth)/login'), 2000);
    } catch (err) {
      showErrorToast('Eroare', err, 'Nu am putut trimite email-ul.');
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
        value={form.email}
        onChangeText={handleChange('email')}
        onBlur={handleBlur('email')}
        error={error}
        touched={touched.email}
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
