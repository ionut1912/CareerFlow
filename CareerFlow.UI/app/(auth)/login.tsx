import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/auth/GradientButton';
import {COLORS} from '@/constants/theme';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {useRouter} from 'expo-router';
import React, {useState} from 'react';
import {StyleSheet, Text, TouchableOpacity} from 'react-native';
import Toast from 'react-native-toast-message';
import {handleAcceptLegal, handleRejectLegal} from './utils';
import {loginThunk} from '@/store/auth/thunks';

const LoginScreen = () => {
  const router = useRouter();
  const dispatch = useAppDispatch();
  const {loading} = useAppSelector(state => state.auth);

  const [legalAccepted, setLegalAccepted] = useState({
    terms: false,
    privacy: false,
  });

  const [form, setForm] = useState({email: '', password: ''});
  const [touched, setTouched] = useState({email: false, password: false});

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  const errors = {
    email: !form.email
      ? 'Email-ul este necesar'
      : !emailRegex.test(form.email)
        ? 'Format invalid'
        : null,
    password: !form.password ? 'Parola este necesara' : null,
  };

  const isFormValid = !errors.email && !errors.password;

  const handleLogin = async () => {
    if (!isFormValid || loading) return;
    try {
      await dispatch(
        loginThunk({email: form.email, password: form.password}),
      ).unwrap();
      router.replace('/(tabs)');
    } catch (error: unknown) {
      Toast.show({
        type: 'error',
        text1: 'Eroare la autentificare',
        text2: error || 'Ceva nu a functionat corect.',
      });
    }
  };

  return (
    <AuthLayout
      title="Career Flow"
      subtitle="Pregateste mintea pentru cunoastere"
      footerText="Nu ai cont?"
      footerActionText="Inregistreaza-te"
      onFooterAction={() => router.replace('/(auth)/register')}
      legalAccepted={legalAccepted}
      onAccept={(type: string) => {
        handleAcceptLegal(type);
        setLegalAccepted(prev => ({...prev, [type]: true}));
      }}
      onReject={(type: string) => {
        handleRejectLegal(type);
        setLegalAccepted(prev => ({...prev, [type]: false}));
      }}>
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
        placeholder="••••••••"
        isPassword
        value={form.password}
        onChangeText={text => setForm({...form, password: text})}
        onBlur={() => setTouched({...touched, password: true})}
        error={errors.password}
        touched={touched.password}
      />
      <TouchableOpacity style={styles.forgotBtn}>
        <Text style={styles.forgotText}>Ai uitat parola?</Text>
      </TouchableOpacity>
      <GradientButton
        text={loading ? 'Se incarca...' : 'Autentificare'}
        icon={loading ? null : 'login'}
        onPress={handleLogin}
        disabled={!isFormValid || loading}
      />
    </AuthLayout>
  );
};

const styles = StyleSheet.create({
  forgotBtn: {alignSelf: 'flex-end', marginBottom: 24},
  forgotText: {color: COLORS.primary, fontSize: 12, fontWeight: '600'},
});

export default LoginScreen;
