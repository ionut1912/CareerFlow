import {COLORS} from '@/constants/theme';
import React from 'react';
import {StyleSheet, Text, TouchableOpacity} from 'react-native';

interface ModalActionButtonProps {
  label: string;
  onPress: () => void;
  variant: 'accept' | 'reject';
}

export const ModalActionButton: React.FC<ModalActionButtonProps> = ({
  label,
  onPress,
  variant,
}) => {
  const isAccept = variant === 'accept';

  return (
    <TouchableOpacity
      style={[styles.btn, isAccept ? styles.btnAccept : styles.btnReject]}
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={label}>
      <Text style={isAccept ? styles.textAccept : styles.textReject}>
        {label}
      </Text>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  btn: {
    flex: 1,
    paddingVertical: 14,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  btnAccept: {
    backgroundColor: COLORS.primary,
  },
  btnReject: {
    backgroundColor: COLORS.inputBg,
    borderWidth: 1,
    borderColor: COLORS.border,
  },
  textAccept: {
    color: COLORS.text,
    fontWeight: '700',
    fontSize: 14,
  },
  textReject: {
    color: COLORS.textSecondary,
    fontWeight: '600',
    fontSize: 14,
  },
});
