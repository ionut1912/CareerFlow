import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/shared/GradientButton';
import {COLORS} from '@/constants/theme';
import {loginThunk} from '@/store/auth/thunks';
import {resetOnboardingThunk} from '@/store/app/thunks'; // NEW IMPORT
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {useRouter} from 'expo-router';
import React from 'react';
import {StyleSheet, Text, TouchableOpacity} from 'react-native';
import {useFormState} from '@/hooks/useFormState';
import {useLegalAcceptance} from '@/hooks/useLegalAcceptance';
import {showErrorToast} from '@/utils/toast';
import {validateEmail, validateRequired} from '@/utils/validators';

const INITIAL_FORM = {email: '', password: ''};

const LoginScreen = () => {
  const router = useRouter();
  const dispatch = useAppDispatch();
  const {loading} = useAppSelector(state => state.auth);

  const {form, touched, handleChange, handleBlur} = useFormState(INITIAL_FORM);
  const {legalAccepted, onAccept, onReject} = useLegalAcceptance();

  const errors = {
    email: validateEmail(form.email),
    password: validateRequired(form.password, 'Parola este necesara'),
  };

  const isFormValid = !errors.email && !errors.password;

  const handleLogin = async () => {
    if (!isFormValid || loading) return;
    try {
      await dispatch(
        loginThunk({email: form.email, password: form.password}),
      ).unwrap();
      router.replace('/(tabs)');
    } catch (error) {
      showErrorToast('Eroare la autentificare', error);
    }
  };

  const handleResetOnboarding = async () => {
    await dispatch(resetOnboardingThunk()).unwrap();
    router.replace('/(onboarding)');
  };

  return (
    <AuthLayout
      title="Career Flow"
      subtitle="Pregateste mintea pentru cunoastere"
      footerText="Nu ai cont?"
      footerActionText="Inregistreaza-te"
      onFooterAction={() => router.replace('/(auth)/register')}
      legalAccepted={legalAccepted}
      onAccept={onAccept}
      onReject={onReject}>
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
        placeholder="••••••••"
        isPassword
        value={form.password}
        onChangeText={handleChange('password')}
        onBlur={handleBlur('password')}
        error={errors.password}
        touched={touched.password}
      />
      <TouchableOpacity
        style={styles.forgotBtn}
        onPress={() => router.push('/(auth)/forgot-password')}>
        <Text style={styles.forgotText}>Ai uitat parola?</Text>
      </TouchableOpacity>

      <GradientButton
        text={loading ? 'Se incarca...' : 'Autentificare'}
        icon={loading ? null : 'login'}
        onPress={handleLogin}
        disabled={!isFormValid || loading}
      />

      {/* DEBUG BUTTON: Reset Onboarding */}
      <TouchableOpacity style={styles.debugBtn} onPress={handleResetOnboarding}>
        <Text style={styles.debugText}>[Debug] Reset Onboarding</Text>
      </TouchableOpacity>
    </AuthLayout>
  );
};

const styles = StyleSheet.create({
  forgotBtn: {alignSelf: 'flex-end', marginBottom: 24},
  forgotText: {color: COLORS.primary, fontSize: 12, fontWeight: '600'},
  debugBtn: {alignSelf: 'center', marginTop: 24, padding: 8},
  debugText: {
    color: COLORS.textMuted || '#9ca3af',
    fontSize: 12,
    textDecorationLine: 'underline',
  },
});

export default LoginScreen;
