import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/shared/GradientButton';
import {RegisterForm, ErrorFields} from '@/models/ui.models';
import {registerThunk} from '@/store/auth/thunks';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {useRouter} from 'expo-router';
import React from 'react';
import {useFormState} from '@/hooks/useFormState';
import {useLegalAcceptance} from '@/hooks/useLegalAcceptance';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {
  validateName,
  validateEmail,
  validatePassword,
  validateRequired,
  validateConfirmPassword,
} from '@/utils/validators';

const INITIAL_FORM: RegisterForm = {
  name: '',
  email: '',
  username: '',
  password: '',
  confirmPassword: '',
};

const RegisterScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const {form, touched, handleChange, handleBlur} =
    useFormState<RegisterForm>(INITIAL_FORM);

  const {legalAccepted, onAccept, onReject, isLegalComplete} =
    useLegalAcceptance();

  const errors: ErrorFields = {
    name: validateName(form.name),
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    username: validateRequired(
      form.username,
      'Numele de utilizator este necesar',
    ),
    confirmPassword: validateConfirmPassword(
      form.password,
      form.confirmPassword,
    ),
  };

  const isFormValid = !Object.values(errors).some(Boolean) && isLegalComplete;

  const handleRegister = async () => {
    if (!isFormValid || isLoading) return;
    try {
      await dispatch(registerThunk(form)).unwrap();
      showSuccessToast('Cont creat cu succes!', 'Te rugam sa te autentifici.');
      setTimeout(() => router.replace('/(auth)/login'), 1500);
    } catch (error) {
      showErrorToast('Eroare la inregistrare', error);
    }
  };

  return (
    <AuthLayout
      title="Acceseaza Career Flow"
      subtitle="Incepe aventura"
      footerText="Ai deja cont?"
      footerActionText="Autentificare"
      onFooterAction={() => router.replace('/(auth)/login')}
      legalAccepted={legalAccepted}
      onAccept={onAccept}
      onReject={onReject}>
      <AppInput
        label="Nume"
        icon="person-outline"
        placeholder="John Doe"
        value={form.name}
        onChangeText={handleChange('name')}
        onBlur={handleBlur('name')}
        error={errors.name}
        touched={touched.name}
      />
      <AppInput
        label="Nume utilizator"
        icon="person-outline"
        placeholder="jdoe"
        value={form.username}
        onChangeText={handleChange('username')}
        onBlur={handleBlur('username')}
        error={errors.username}
        touched={touched.username}
      />
      <AppInput
        label="Adresa de email"
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
        label="Parola"
        icon="lock-outline"
        placeholder="Parola"
        isPassword
        value={form.password}
        onChangeText={handleChange('password')}
        onBlur={handleBlur('password')}
        error={errors.password}
        touched={touched.password}
      />
      <AppInput
        label="Confirma parola"
        icon="lock-outline"
        placeholder="Confirma parola"
        isPassword
        value={form.confirmPassword}
        onChangeText={handleChange('confirmPassword')}
        onBlur={handleBlur('confirmPassword')}
        error={errors.confirmPassword}
        touched={touched.confirmPassword}
      />
      <GradientButton
        text={isLoading ? 'Se incarca...' : 'Creare cont'}
        icon={isLoading ? null : 'person-add'}
        onPress={handleRegister}
        disabled={!isFormValid || isLoading}
      />
    </AuthLayout>
  );
};

export default RegisterScreen;
