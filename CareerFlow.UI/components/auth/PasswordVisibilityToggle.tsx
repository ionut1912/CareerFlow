import {COLORS} from '@/constants/theme';
import {MaterialIcons} from '@expo/vector-icons';
import React from 'react';
import {TouchableOpacity} from 'react-native';

interface PasswordVisibilityToggleProps {
  isVisible: boolean;
  onToggle: () => void;
}

export const PasswordVisibilityToggle: React.FC<
  PasswordVisibilityToggleProps
> = ({isVisible, onToggle}) => (
  <TouchableOpacity
    onPress={onToggle}
    accessibilityRole="button"
    accessibilityLabel={isVisible ? 'Ascunde parola' : 'Arata parola'}>
    <MaterialIcons
      name={isVisible ? 'visibility' : 'visibility-off'}
      size={20}
      color={COLORS.textMuted}
    />
  </TouchableOpacity>
);
