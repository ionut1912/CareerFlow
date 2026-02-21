import { AppInput } from '@/components/auth/AppInput';
import { AuthLayout } from '@/components/auth/AuthLayout';
import { GradientButton } from '@/components/auth/GradientButton';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';
import Toast from 'react-native-toast-message';
import { handleAcceptLegal, handleRejectLegal } from './utils';
import { ErrorFields, RegisterForm, TouchedFields } from '@/models/ui.models';
import { register } from '@/services/authService';
import { isAxiosError } from 'axios';

interface ApiErrorResponse {
  message: string;
}

const RegisterScreen = () => {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [legalAccepted, setLegalAccepted] = useState({
    terms: false,
    privacy: false,
  });

  const [form, setForm] = useState<RegisterForm>({
    name: '',
    email: '',
    username: '',
    password: '',
    confirmPassword: '',
  });
const eroareIntentionata = "husky trebuddsffsdsfie sa maadsadsdsds blochezesdf aici1sdasdaa";
  const [touched, setTouched] = useState<TouchedFields>({
    name: false,
    email: false,
    password: false,
    username: false,
    confirmPassword: false,
  });

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  const errors: ErrorFields = {
    name: !form.name
      ? 'Numele este necesar'
      : form.name.trim().length < 2
        ? 'Numele trebuie sa aiba minim 2 caractere'
        : null,
    email: !form.email
      ? 'Email-ul este necesar'
      : !emailRegex.test(form.email)
        ? 'Format invalid'
        : null,
    password: !form.password
      ? 'Parola este necesara'
      : form.password.length < 6
        ? 'Parola trebuie sa aiba minim 6 caractere'
        : null,
    username: !form.username ? 'Numele de utilizator este necesar' : null,
    confirmPassword: !form.confirmPassword
      ? 'Confirmarea parolei este necesara'
      : form.confirmPassword !== form.password
        ? 'Parolele nu se potrivesc'
        : null,
  };

  const isFormValid = 
    !errors.name && 
    !errors.email && 
    !errors.password && 
    !errors.username && 
    !errors.confirmPassword &&
    legalAccepted.terms && 
    legalAccepted.privacy;

  const handleRegister = async () => {
    if (!isFormValid || isLoading) return;

    setIsLoading(true);

    try {
      await register(form);

      Toast.show({
        type: 'success',
        text1: 'Cont creat cu succes!',
        text2: 'Te rugam sa te autentifici.',
      });

      setTimeout(() => {
        router.replace('/(auth)/login');
      }, 1500);
    } catch (error: unknown) {
      let errorMessage = 'Ceva nu a functionat corect. Incearca din nou.';

      if (isAxiosError<ApiErrorResponse>(error))  {
        errorMessage = error.response?.data?.message || errorMessage;
      }

      Toast.show({
        type: 'error',
        text1: 'Eroare la inregistrare',
        text2: errorMessage,
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Acceseaza Career Flow"
      subtitle="Incepe aventura"
      footerText="Ai deja cont?"
      footerActionText="Autentificare"
      onFooterAction={() => router.replace('/(auth)/login')}
      onAccept={(type: string) => {
        handleAcceptLegal(type);
        setLegalAccepted(prev => ({ ...prev, [type]: true }));
      }}
      onReject={(type: string) => {
        handleRejectLegal(type);
        setLegalAccepted(prev => ({ ...prev, [type]: false }));
      }}
    >
      <AppInput
        label="Nume"
        icon="person-outline"
        placeholder="John Doe"
        value={form.name}
        onChangeText={(text: string) => setForm({ ...form, name: text })}
        onBlur={() => setTouched({ ...touched, name: true })}
        error={errors.name}
        touched={touched.name}
      />
      <AppInput
        label="Nume utilizator"
        icon="person-outline"
        placeholder="jdoe"
        value={form.username}
        onChangeText={(text: string) => setForm({ ...form, username: text })}
        onBlur={() => setTouched({ ...touched, username: true })}
        error={errors.username}
        touched={touched.username}
      />
      <AppInput
        label="Adresa de email"
        icon="mail-outline"
        placeholder="you@example.com"
        keyboardType="email-address"
        value={form.email}
        onChangeText={(text: string) => setForm({ ...form, email: text })}
        onBlur={() => setTouched({ ...touched, email: true })}
        error={errors.email}
        touched={touched.email}
      />
      <AppInput
        label="Parola"
        icon="lock-outline"
        placeholder="Parola"
        isPassword
        value={form.password}
        onChangeText={(text: string) => setForm({ ...form, password: text })}
        onBlur={() => setTouched({ ...touched, password: true })}
        error={errors.password}
        touched={touched.password}
      />
      <AppInput
        label="Confirma parola"
        icon="lock-outline"
        placeholder="Confirma parola"
        isPassword
        value={form.confirmPassword}
        onChangeText={(text: string) => setForm({ ...form, confirmPassword: text })}
        onBlur={() => setTouched({ ...touched, confirmPassword: true })}
        error={errors.confirmPassword || (form.confirmPassword !== form.password ? 'Parolele nu se potrivesc' : null)}
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