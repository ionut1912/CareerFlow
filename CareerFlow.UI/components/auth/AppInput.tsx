import {COLORS} from '@/constants/theme';
import {MaterialIcons} from '@expo/vector-icons';
import React, {useState} from 'react';
import {
  StyleSheet,
  Text,
  TextInput,
  TextInputProps,
  TouchableOpacity,
  View,
} from 'react-native';

interface AppInputProps extends TextInputProps {
  label: string;
  icon: keyof typeof MaterialIcons.glyphMap;
  error?: string | null;
  touched?: boolean;
  isPassword?: boolean;
}

export const AppInput: React.FC<AppInputProps> = ({
  label,
  icon,
  value,
  onChangeText,
  onBlur,
  error,
  touched,
  isPassword = false,
  placeholder,
  ...props
}) => {
  const [isVisible, setIsVisible] = useState(false);
  const showError = touched && error;

  return (
    <View style={styles.container}>
      <Text style={styles.label}>{label}</Text>
      <View style={[styles.wrapper, !!showError && styles.wrapperError]}>
        <MaterialIcons
          name={icon}
          size={20}
          color={showError ? COLORS.error : COLORS.textMuted}
          style={styles.icon}
          importantForAccessibility="no-hide-descendants"
        />
        <TextInput
          style={styles.input}
          placeholder={placeholder}
          placeholderTextColor={COLORS.textMuted}
          secureTextEntry={isPassword && !isVisible}
          value={value}
          onChangeText={onChangeText}
          onBlur={onBlur}
          autoCapitalize="none"
          accessibilityLabel={label}
          {...props}
        />
        {isPassword && (
          <TouchableOpacity
            onPress={() => setIsVisible(!isVisible)}
            accessibilityRole="button"
            accessibilityLabel={isVisible ? 'Ascunde parola' : 'Arata parola'}>
            <MaterialIcons
              name={isVisible ? 'visibility' : 'visibility-off'}
              size={20}
              color={COLORS.textMuted}
            />
          </TouchableOpacity>
        )}
      </View>
      {!!showError && (
        <Text
          style={styles.errorText}
          accessibilityLiveRegion="polite"
          accessibilityRole="alert">
          {error}
        </Text>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {marginBottom: 16},
  label: {
    color: '#d1d5db',
    fontSize: 12,
    fontWeight: '600',
    marginBottom: 6,
    marginLeft: 4,
  },
  wrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.inputBg,
    borderWidth: 1,
    borderColor: COLORS.border,
    borderRadius: 12,
    paddingHorizontal: 12,
    height: 52,
  },
  wrapperError: {
    borderColor: COLORS.error,
    backgroundColor: 'rgba(239, 68, 68, 0.05)',
  },
  input: {flex: 1, color: COLORS.text, fontSize: 14},
  icon: {marginRight: 10},
  errorText: {
    color: COLORS.error,
    fontSize: 11,
    marginTop: 4,
    marginLeft: 4,
    fontWeight: '500',
  },
});
