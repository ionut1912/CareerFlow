import {useSocialAuth} from '@/hooks/useSocialAuth';
import {useAppSelector} from '@/store/hook';
import {FontAwesome} from '@expo/vector-icons';
import React, {useRef, useState} from 'react';
import {Animated, StyleSheet, Text, View} from 'react-native';
import {SocialButton} from './SocialButton';

interface SocialLoginButtonsProps {
  legalAccepted?: {
    terms: boolean;
    privacy: boolean;
  };
}

const TOOLTIP_COLORS = {
  amber: '#f59e0b',
  bg: 'rgba(245, 158, 11, 0.12)',
  border: 'rgba(245, 158, 11, 0.35)',
};

const MISSING_LEGAL_MESSAGES: Record<string, string> = {
  both: 'Acceptă Termenii și Politica de confidențialitate pentru a continua.',
  terms: 'Acceptă Termenii și condițiile pentru a continua.',
  privacy: 'Acceptă Politica de confidențialitate pentru a continua.',
};

function getMissingLegalText(termsOk: boolean, privacyOk: boolean): string {
  if (!termsOk && !privacyOk) return MISSING_LEGAL_MESSAGES.both;
  if (!termsOk) return MISSING_LEGAL_MESSAGES.terms;
  if (!privacyOk) return MISSING_LEGAL_MESSAGES.privacy;
  return '';
}

export default function SocialLoginButtons({
  legalAccepted,
}: SocialLoginButtonsProps) {
  const {loginWithGoogle, loginWithLinkedin} = useSocialAuth();
  const loading = useAppSelector(state => state.auth.loading);
  const [tooltipVisible, setTooltipVisible] = useState(false);
  const tooltipOpacity = useRef(new Animated.Value(0)).current;

  const termsOk = legalAccepted?.terms ?? false;
  const privacyOk = legalAccepted?.privacy ?? false;
  const legalComplete = termsOk && privacyOk;

  const showLegalTooltip = () => {
    if (tooltipVisible) return;

    setTooltipVisible(true);
    Animated.sequence([
      Animated.timing(tooltipOpacity, {
        toValue: 1,
        duration: 200,
        useNativeDriver: true,
      }),
      Animated.delay(2500),
      Animated.timing(tooltipOpacity, {
        toValue: 0,
        duration: 300,
        useNativeDriver: true,
      }),
    ]).start(() => setTooltipVisible(false));
  };

  const guardedPress = (action: () => void) => () => {
    if (!legalComplete) {
      showLegalTooltip();
      return;
    }
    action();
  };

  const SOCIAL_PROVIDERS: {
    label: string;
    icon: React.ComponentProps<typeof FontAwesome>['name'];
    onPress: () => void;
  }[] = [
    {label: 'Google', icon: 'google', onPress: guardedPress(loginWithGoogle)},
    {
      label: 'LinkedIn',
      icon: 'linkedin-square',
      onPress: guardedPress(loginWithLinkedin),
    },
  ];

  return (
    <View style={styles.container}>
      {tooltipVisible && !legalComplete && (
        <Animated.View
          style={[styles.tooltip, {opacity: tooltipOpacity}]}
          accessibilityLiveRegion="assertive"
          accessibilityRole="alert">
          <FontAwesome
            name="exclamation-circle"
            size={13}
            color={TOOLTIP_COLORS.amber}
            style={styles.tooltipIcon}
          />
          <Text style={styles.tooltipText}>
            {getMissingLegalText(termsOk, privacyOk)}
          </Text>
        </Animated.View>
      )}

      {SOCIAL_PROVIDERS.map(({label, icon, onPress}, index) => (
        <SocialButton
          key={label}
          label={label}
          icon={icon}
          onPress={onPress}
          visuallyDisabled={!legalComplete}
          loading={loading}
          style={index > 0 ? styles.marginTop : undefined}
        />
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {marginTop: 24},
  marginTop: {marginTop: 12},
  tooltip: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: TOOLTIP_COLORS.bg,
    borderWidth: 1,
    borderColor: TOOLTIP_COLORS.border,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 8,
    marginBottom: 12,
  },
  tooltipIcon: {marginRight: 6},
  tooltipText: {
    color: TOOLTIP_COLORS.amber,
    fontSize: 12,
    flex: 1,
    lineHeight: 16,
  },
});
