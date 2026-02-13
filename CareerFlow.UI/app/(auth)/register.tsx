import { AppInput } from '@/components/auth/AppInput';
import { AuthLayout } from '@/components/auth/AuthLayout';
import { GradientButton } from '@/components/auth/GradientButton';
import { useRouter } from 'expo-router';
import React, { useState } from 'react';

const RegisterScreen = () => {
  const router = useRouter();
  const [form, setForm] = useState({
    name: '',
    email: '',
    username: '',
    password: '',
  });
  const [touched, setTouched] = useState({
    name: false,
    email: false,
    password: false,
    username: false,
  });

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  const errors = {
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
  };

  const isFormValid =
    !errors.name && !errors.email && !errors.password && !errors.username;

  const handleRegister = () => {
    if (isFormValid) {
      console.log('Register Success:', form);
    }
  };

  return (
    <AuthLayout
      title="Acceseaza Career Flow"
      subtitle="Incepe aventura"
      footerText="Ai deja cont?"
      footerActionText="Autentificare"
      onFooterAction={() => router.replace('/(auth)/login')}>
      <AppInput
        label="Nume"
        icon="person-outline"
        placeholder="John Doe"
        value={form.name}
        onChangeText={text => setForm({...form, name: text})}
        onBlur={() => setTouched({...touched, name: true})}
        error={errors.name}
        touched={touched.name}
      />
      <AppInput
        label="Nume utilizator"
        icon="person-outline"
        placeholder="jdoe"
        value={form.username}
        onChangeText={text => setForm({...form, username: text})}
        onBlur={() => setTouched({...touched, username: true})}
        error={errors.username}
        touched={touched.username}
      />
      <AppInput
        label="Adresa de email"
        icon="mail-outline"
        placeholder="you@example.com"
        keyboardType="email-address"
        value={form.email}
        onChangeText={text => setForm({...form, email: text})}
        onBlur={() => setTouched({...touched, email: true})}
        error={errors.email}
        touched={touched.email}
      />
      <AppInput
        label="Parola"
        icon="lock-outline"
        placeholder="Min 6 characters"
        isPassword
        value={form.password}
        onChangeText={text => setForm({...form, password: text})}
        onBlur={() => setTouched({...touched, password: true})}
        error={errors.password}
        touched={touched.password}
      />

      <GradientButton
        text="Creare cont"
        icon="person-add"
        onPress={handleRegister}
        disabled={!isFormValid}
      />
    </AuthLayout>
  );
};

export default RegisterScreen;
