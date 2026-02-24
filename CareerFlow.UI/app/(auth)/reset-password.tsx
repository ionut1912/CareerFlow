import {AppInput} from '@/components/auth/AppInput';
import {AuthLayout} from '@/components/auth/AuthLayout';
import {GradientButton} from '@/components/auth/GradientButton';
import {useRouter, useLocalSearchParams} from 'expo-router';
import React, {useState} from 'react';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {resetPasswordThunk} from '@/store/auth/thunks';

const ResetPasswordScreen = () => {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const {token} = useLocalSearchParams<{token: string}>();
  const {loading: isLoading} = useAppSelector(state => state.auth);

  const [form, setForm] = useState({
    password: '',
    confirmPassword: '',
    email: '',
  });

  const handleResetPassword = async () => {
    if (form.password !== form.confirmPassword || isLoading) return;

    try {
      await dispatch(
        resetPasswordThunk({
          email: form.email,
          newPassword: form.password,
          token: token,
        }),
      ).unwrap();

      Toast.show({type: 'success', text1: 'Parolă resetată!'});
      setTimeout(() => router.replace('/(auth)/login'), 1500);
    } catch {
      Toast.show({type: 'error', text1: 'Eroare la resetare'});
    }
  };

  return (
    <AuthLayout title="Resetare Parolă">
      <AppInput
        label="Email"
        icon="mail-outline"
        value={form.email}
        onChangeText={text => setForm({...form, email: text})}
      />
      <AppInput
        label="Noua Parolă"
        icon="lock-closed-outline"
        isPassword
        value={form.password}
        onChangeText={text => setForm({...form, password: text})}
      />
      <AppInput
        label="Confirmă Parola"
        icon="lock-closed-outline"
        isPassword
        value={form.confirmPassword}
        onChangeText={text => setForm({...form, confirmPassword: text})}
      />
      <GradientButton
        text={isLoading ? 'Se procesează...' : 'Resetează Parola'}
        onPress={handleResetPassword}
      />
    </AuthLayout>
  );
};

export default ResetPasswordScreen;
