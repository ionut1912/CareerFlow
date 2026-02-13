import SocialLoginButtons from '@/components/SocialLoginButtons';
import { COLORS, STYLES } from '@/constants/theme';
import { MaterialIcons } from '@expo/vector-icons';
import { usePathname, useRouter } from 'expo-router';
import React from 'react';
import {
    KeyboardAvoidingView,
    Platform,
    SafeAreaView,
    ScrollView,
    StyleSheet,
    Text,
    TouchableOpacity,
    View,
} from 'react-native';

interface AuthLayoutProps {
  children: React.ReactNode;
  title: string;
  subtitle: string;
  footerText: string;
  footerActionText: string;
  onFooterAction: () => void;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({
  children,
  title,
  subtitle,
  footerText,
  footerActionText,
  onFooterAction,
}) => {
  const router = useRouter();
  const pathname = usePathname();
  const isLogin = pathname.includes('login');

  return (
    <View style={styles.container}>
      <View
        style={[
          STYLES.glow,
          {top: -50, left: -50, backgroundColor: COLORS.primary},
        ]}
      />
      <View
        style={[
          STYLES.glow,
          {bottom: -50, right: -50, backgroundColor: '#3b82f6'},
        ]}
      />

      <SafeAreaView style={{flex: 1}}>
        <KeyboardAvoidingView
          behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
          style={{flex: 1}}>
          <ScrollView
            contentContainerStyle={styles.scrollContent}
            showsVerticalScrollIndicator={false}>
            <View style={styles.header}>
              <View style={styles.logoContainer}>
                <View style={styles.logoGlow} />
                <View style={styles.logoBox}>
                  <MaterialIcons
                    name="psychology"
                    size={40}
                    color={COLORS.primary}
                  />
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
                  title="Creare Cont"
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
              <View style={styles.legalRow}>
                <Text style={styles.legalText}>
                  Politica de confidentialitate • Termeni si conditii
                </Text>
              </View>
            </View>
          </ScrollView>
        </KeyboardAvoidingView>
      </SafeAreaView>
    </View>
  );
};

interface TabButtonProps {
  title: string;
  active: boolean;
  onPress: () => void;
}

const TabButton: React.FC<TabButtonProps> = ({title, active, onPress}) => (
  <TouchableOpacity
    style={[styles.tab, active && styles.activeTab]}
    onPress={onPress}>
    <Text style={[styles.tabText, active && styles.activeTabText]}>
      {title}
    </Text>
  </TouchableOpacity>
);

const styles = StyleSheet.create({
  container: {flex: 1, backgroundColor: COLORS.background},
  scrollContent: {paddingHorizontal: 24, paddingBottom: 40},
  header: {alignItems: 'center', marginTop: 60, marginBottom: 32},
  logoContainer: {
    width: 80,
    height: 80,
    justifyContent: 'center',
    alignItems: 'center',
  },
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
  title: {fontSize: 28, fontWeight: '700', color: COLORS.text, marginTop: 16},
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
  tab: {flex: 1, paddingVertical: 10, alignItems: 'center', borderRadius: 8},
  activeTab: {backgroundColor: COLORS.primary},
  tabText: {color: COLORS.textSecondary, fontSize: 14, fontWeight: '600'},
  activeTabText: {color: COLORS.text},
  dividerRow: {flexDirection: 'row', alignItems: 'center', marginVertical: 24},
  divider: {flex: 1, height: 1, backgroundColor: COLORS.border},
  dividerText: {
    color: COLORS.textMuted,
    fontSize: 10,
    marginHorizontal: 10,
    fontWeight: '600',
  },
  footer: {marginTop: 32, alignItems: 'center'},
  footerMainText: {color: COLORS.textMuted, fontSize: 12},
  linkText: {color: COLORS.primary, fontWeight: '600'},
  legalRow: {flexDirection: 'row', marginTop: 16},
  legalText: {color: '#4b5563', fontSize: 10},
});
