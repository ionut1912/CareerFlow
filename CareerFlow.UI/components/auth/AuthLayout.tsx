import React, { useState } from 'react';
import {
  View,
  Text,
  SafeAreaView,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
  StyleSheet,
  TouchableOpacity,
} from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';
import { useRouter, usePathname } from 'expo-router';
import { COLORS, STYLES } from '@/constants/theme';
import SocialLoginButtons from '@/components/SocialLoginButtons';
import { TabButton } from '../TabButton';
import { LegalModal } from '../LegalModal';
import axios from 'axios';

interface AuthLayoutProps {
  children: React.ReactNode;
  title: string;
  subtitle: string;
  footerText: string;
  footerActionText: string;
  onFooterAction: () => void;
  onReject: (type: string) => void;
  onAccept: (type: string) => void;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({
  children,
  title,
  subtitle,
  footerText,
  footerActionText,
  onFooterAction,
  onReject,
  onAccept,
}) => {
  const router = useRouter();
  const pathname = usePathname();
  const isLogin = pathname.includes('login');

  const [modal, setModal] = useState({
    visible: false,
    loading: false,
    title: '',
    content: '',
    type: '',
  });

  const fetchLegal = async (type: 'privacy' | 'terms') => {
    setModal((prev) => ({ ...prev, visible: true, loading: true, type }));
    try {
      const res = await axios.get(`https://www.carerflow-api.ro/legal?type=${type}`);
      const data = res.data;
      setModal((prev) => ({
        ...prev,
        loading: false,
        title: type === 'privacy' ? 'Politica de Confidențialitate' : 'Termeni și Condiții',
        content: data.content,
      }));
    } catch {
      setModal((prev) => ({
        ...prev,
        loading: false,
        title: 'Eroare',
        content: 'Eroare la încărcarea datelor din backend.',
      }));
    }
  };

  const handleAccept = () => {
    onAccept(modal.type);
    setModal((prev) => ({ ...prev, visible: false }));
  };

  const handleReject = () => {
    onReject(modal.type);
    setModal((prev) => ({ ...prev, visible: false }));
  };

  return (
    <View style={styles.container}>
      <View style={[STYLES.glow, { top: -50, left: -50, backgroundColor: COLORS.primary }]} />
      <View style={[STYLES.glow, { bottom: -50, right: -50, backgroundColor: '#3b82f6' }]} />

      <SafeAreaView style={{ flex: 1 }}>
        <KeyboardAvoidingView
          behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
          style={{ flex: 1 }}>
          <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
            <View style={styles.header}>
              <View style={styles.logoContainer}>
                <View style={styles.logoGlow} />
                <View style={styles.logoBox}>
                  <MaterialIcons name="psychology" size={40} color={COLORS.primary} />
                </View>
              </View>
              <Text style={styles.title}>{title}</Text>
              <Text style={styles.subtitle}>{subtitle}</Text>
            </View>

            <View style={styles.card}>
              <View style={styles.tabBar}>
                <TabButton
                  title="Inregistrare"
                  active={!isLogin}
                  onPress={() => router.replace('/(auth)/register')}
                />
                <TabButton
                  title="Autentificare"
                  active={isLogin}
                  onPress={() => router.replace('/(auth)/login')}
                />
              </View>
              {children}
              <View style={styles.dividerRow}>
                <View style={styles.divider} />
                <Text style={styles.dividerText}>SAU CONTINUA CU</Text>
                <View style={styles.divider} />
              </View>
              <SocialLoginButtons />
            </View>

            <View style={styles.footer}>
              <Text style={styles.footerMainText}>
                {footerText}{' '}
                <Text style={styles.linkText} onPress={onFooterAction}>
                  {footerActionText}
                </Text>
              </Text>
              <View style={styles.legalLinks}>
                <TouchableOpacity onPress={() => fetchLegal('privacy')}>
                  <Text style={styles.legalItem}>Politica de confidențialitate</Text>
                </TouchableOpacity>
                <Text style={styles.sep}> • </Text>
                <TouchableOpacity onPress={() => fetchLegal('terms')}>
                  <Text style={styles.legalItem}>Termeni și condiții</Text>
                </TouchableOpacity>
              </View>
            </View>
          </ScrollView>
        </KeyboardAvoidingView>
      </SafeAreaView>

      <LegalModal
        visible={modal.visible}
        loading={modal.loading}
        title={modal.title}
        content={modal.content}
        onClose={() => setModal((prev) => ({ ...prev, visible: false }))}
        onAccept={handleAccept}
        onReject={handleReject}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  scrollContent: { paddingHorizontal: 24, paddingBottom: 40 },
  header: { alignItems: 'center', marginTop: 60, marginBottom: 32 },
  logoContainer: { width: 80, height: 80, justifyContent: 'center', alignItems: 'center' },
  logoBox: {
    backgroundColor: COLORS.background,
    borderWidth: 1,
    borderColor: 'rgba(175, 37, 244, 0.3)',
    borderRadius: 16,
    width: '100%',
    height: '100%',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 2,
  },
  logoGlow: {
    position: 'absolute',
    width: 60,
    height: 60,
    backgroundColor: COLORS.primary,
    borderRadius: 30,
    opacity: 0.4,
  },
  title: { fontSize: 28, fontWeight: '700', color: COLORS.text, marginTop: 16 },
  subtitle: {
    fontSize: 12,
    color: COLORS.textSecondary,
    fontWeight: '600',
    letterSpacing: 1,
    marginTop: 4,
  },
  card: {
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderRadius: 24,
    padding: 24,
    borderWidth: 1,
    borderColor: COLORS.border,
  },
  tabBar: {
    flexDirection: 'row',
    backgroundColor: COLORS.inputBg,
    padding: 4,
    borderRadius: 12,
    marginBottom: 24,
  },
  dividerRow: { flexDirection: 'row', alignItems: 'center', marginVertical: 24 },
  divider: { flex: 1, height: 1, backgroundColor: COLORS.border },
  dividerText: {
    color: COLORS.textMuted,
    fontSize: 10,
    marginHorizontal: 10,
    fontWeight: '600',
  },
  footer: { marginTop: 32, alignItems: 'center' },
  footerMainText: { color: COLORS.textMuted, fontSize: 12 },
  linkText: { color: COLORS.primary, fontWeight: '600' },
  legalLinks: { flexDirection: 'row', marginTop: 16, alignItems: 'center' },
  legalItem: { color: COLORS.textSecondary, fontSize: 11, textDecorationLine: 'underline' },
  sep: { color: COLORS.textMuted, fontSize: 11 },
});