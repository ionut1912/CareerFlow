import React from 'react';
import {
  ActivityIndicator,
  StyleSheet,
  Text,
  TouchableOpacity,
  ViewStyle,
} from 'react-native';
import {FontAwesome} from '@expo/vector-icons';

const SOCIAL_COLORS = {
  btnBg: 'rgba(255, 255, 255, 0.05)',
  btnBorder: 'rgba(255, 255, 255, 0.1)',
  btnBgDisabled: 'rgba(255, 255, 255, 0.02)',
  btnBorderDisabled: 'rgba(255, 255, 255, 0.05)',
  btnText: '#e5e7eb',
  btnTextDisabled: 'rgba(255, 255, 255, 0.25)',
  iconActive: 'white',
  iconDisabled: 'rgba(255,255,255,0.3)',
};

interface SocialButtonProps {
  label: string;
  icon: React.ComponentProps<typeof FontAwesome>['name'];
  onPress: () => void;
  disabled?: boolean;
  loading?: boolean;
  style?: ViewStyle;
  visuallyDisabled?: boolean;
}

export const SocialButton: React.FC<SocialButtonProps> = ({
  label,
  icon,
  onPress,
  disabled = false,
  loading = false,
  style,
  visuallyDisabled = false,
}) => {
  // Determine if it should look disabled
  const showAsDisabled = disabled || visuallyDisabled;
  const iconColor = showAsDisabled
    ? SOCIAL_COLORS.iconDisabled
    : SOCIAL_COLORS.iconActive;

  return (
    <TouchableOpacity
      style={[styles.btn, showAsDisabled && styles.btnDisabled, style]}
      onPress={onPress}
      // ONLY physically block touches if it's strictly disabled or currently loading
      disabled={disabled || loading}
      activeOpacity={showAsDisabled ? 1 : 0.7}
      accessibilityRole="button"
      accessibilityLabel={`Continuă cu ${label}`}
      accessibilityState={{disabled: disabled || loading, busy: loading}}
      accessibilityLiveRegion="polite">
      {loading ? (
        <ActivityIndicator color="white" />
      ) : (
        <>
          <FontAwesome
            name={icon}
            size={20}
            color={iconColor}
            style={styles.icon}
          />
          <Text style={[styles.text, showAsDisabled && styles.textDisabled]}>
            {label}
          </Text>
        </>
      )}
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  btn: {
    flexDirection: 'row',
    height: 52,
    backgroundColor: SOCIAL_COLORS.btnBg,
    borderWidth: 1,
    borderColor: SOCIAL_COLORS.btnBorder,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
  },
  btnDisabled: {
    backgroundColor: SOCIAL_COLORS.btnBgDisabled,
    borderColor: SOCIAL_COLORS.btnBorderDisabled,
  },
  icon: {marginRight: 10},
  text: {color: SOCIAL_COLORS.btnText, fontSize: 14, fontWeight: '600'},
  textDisabled: {color: SOCIAL_COLORS.btnTextDisabled},
});
