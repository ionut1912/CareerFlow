import { COLORS } from '@/constants/theme';
import { MaterialIcons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import React from 'react';
import { StyleSheet, Text, TouchableOpacity } from 'react-native';

interface GradientButtonProps {
  onPress: () => void;
  text: string;
  icon?: keyof typeof MaterialIcons.glyphMap;
  disabled?: boolean;
}

export const GradientButton: React.FC<GradientButtonProps> = ({
  onPress,
  text,
  icon,
  disabled,
}) => (
  <TouchableOpacity
    style={[styles.container, disabled && {opacity: 0.5}]}
    activeOpacity={0.8}
    disabled={disabled}
    onPress={onPress}
    accessibilityRole="button"
    accessibilityLabel={text}
    accessibilityState={{disabled: !!disabled}}>
    <LinearGradient
      colors={[COLORS.primary, COLORS.primaryDark]}
      style={styles.gradient}>
      {icon && (
        <MaterialIcons
          name={icon}
          size={20}
          color="white"
          style={{marginRight: 8}}
        />
      )}
      <Text style={styles.text}>{text}</Text>
    </LinearGradient>
  </TouchableOpacity>
);

const styles = StyleSheet.create({
  container: {
    borderRadius: 12,
    overflow: 'hidden',
    marginTop: 10,
    shadowColor: COLORS.primary,
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
  },
  gradient: {
    flexDirection: 'row',
    height: 52,
    justifyContent: 'center',
    alignItems: 'center',
  },
  text: {color: 'white', fontSize: 16, fontWeight: '700'},
});
