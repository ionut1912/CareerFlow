import {useSocialAuth} from '@/hooks/useSocialAuth';
import {useAppSelector} from '@/store/hook';
import {FontAwesome} from '@expo/vector-icons';
import React, {useState} from 'react';
import {
  ActivityIndicator,
  Animated,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';

interface SocialLoginButtonsProps {
  legalAccepted?: {
    terms: boolean;
    privacy: boolean;
  };
}

const SOCIAL_COLORS = {
  amber: '#f59e0b',
  tooltipBg: 'rgba(245, 158, 11, 0.12)',
  tooltipBorder: 'rgba(245, 158, 11, 0.35)',
  btnBg: 'rgba(255, 255, 255, 0.05)',
  btnBorder: 'rgba(255, 255, 255, 0.1)',
  btnBgDisabled: 'rgba(255, 255, 255, 0.02)',
  btnBorderDisabled: 'rgba(255, 255, 255, 0.05)',
  btnText: '#e5e7eb',
  btnTextDisabled: 'rgba(255, 255, 255, 0.25)',
  iconActive: 'white',
  iconDisabled: 'rgba(255,255,255,0.3)',
};

export default function SocialLoginButtons({
  legalAccepted,
}: SocialLoginButtonsProps) {
  const {loginWithGoogle, loginWithLinkedin} = useSocialAuth();
  const loading = useAppSelector(state => state.auth.loading);
  const [tooltipVisible, setTooltipVisible] = useState(false);
  const tooltipOpacity = React.useRef(new Animated.Value(0)).current;

  const termsOk = legalAccepted?.terms ?? false;
  const privacyOk = legalAccepted?.privacy ?? false;
  const legalComplete = termsOk && privacyOk;

  const showLegalTooltip = () => {
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

  const handleGooglePress = () => {
    if (!legalComplete) {
      showLegalTooltip();
      return;
    }
    loginWithGoogle();
  };

  const handleLinkedinPress = () => {
    if (!legalComplete) {
      showLegalTooltip();
      return;
    }
    loginWithLinkedin();
  };

  const getMissingLegalText = () => {
    if (!termsOk && !privacyOk)
      return 'Acceptă Termenii și Politica de confidențialitate pentru a continua.';
    if (!termsOk) return 'Acceptă Termenii și condițiile pentru a continua.';
    if (!privacyOk)
      return 'Acceptă Politica de confidențialitate pentru a continua.';
    return '';
  };

  const iconColor = legalComplete
    ? SOCIAL_COLORS.iconActive
    : SOCIAL_COLORS.iconDisabled;

  return (
    <View style={styles.container}>
      {tooltipVisible && !legalComplete && (
        <Animated.View style={[styles.tooltip, {opacity: tooltipOpacity}]}>
          <FontAwesome
            name="exclamation-circle"
            size={13}
            color={SOCIAL_COLORS.amber}
            style={styles.tooltipIcon}
          />
          <Text style={styles.tooltipText}>{getMissingLegalText()}</Text>
        </Animated.View>
      )}

      <TouchableOpacity
        style={[styles.socialBtn, !legalComplete && styles.socialBtnDisabled]}
        onPress={handleGooglePress}
        activeOpacity={legalComplete ? 0.7 : 1}>
        {loading ? (
          <ActivityIndicator color="white" />
        ) : (
          <>
            <FontAwesome
              name="google"
              size={20}
              color={iconColor}
              style={styles.socialIcon}
            />
            <Text
              style={[
                styles.socialBtnText,
                !legalComplete && styles.socialBtnTextDisabled,
              ]}>
              Google
            </Text>
          </>
        )}
      </TouchableOpacity>

      <TouchableOpacity
        style={[
          styles.socialBtn,
          styles.socialBtnMargin,
          !legalComplete && styles.socialBtnDisabled,
        ]}
        onPress={handleLinkedinPress}
        activeOpacity={legalComplete ? 0.7 : 1}>
        {loading ? (
          <ActivityIndicator color="white" />
        ) : (
          <>
            <FontAwesome
              name="linkedin-square"
              size={20}
              color={iconColor}
              style={styles.socialIcon}
            />
            <Text
              style={[
                styles.socialBtnText,
                !legalComplete && styles.socialBtnTextDisabled,
              ]}>
              LinkedIn
            </Text>
          </>
        )}
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {marginTop: 24},
  tooltip: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: SOCIAL_COLORS.tooltipBg,
    borderWidth: 1,
    borderColor: SOCIAL_COLORS.tooltipBorder,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 8,
    marginBottom: 12,
  },
  tooltipIcon: {marginRight: 6},
  tooltipText: {
    color: SOCIAL_COLORS.amber,
    fontSize: 12,
    flex: 1,
    lineHeight: 16,
  },
  socialBtn: {
    flexDirection: 'row',
    height: 52,
    backgroundColor: SOCIAL_COLORS.btnBg,
    borderWidth: 1,
    borderColor: SOCIAL_COLORS.btnBorder,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
  },
  socialBtnDisabled: {
    backgroundColor: SOCIAL_COLORS.btnBgDisabled,
    borderColor: SOCIAL_COLORS.btnBorderDisabled,
  },
  socialBtnMargin: {marginTop: 12},
  socialIcon: {marginRight: 10},
  socialBtnText: {
    color: SOCIAL_COLORS.btnText,
    fontSize: 14,
    fontWeight: '600',
  },
  socialBtnTextDisabled: {color: SOCIAL_COLORS.btnTextDisabled},
});
