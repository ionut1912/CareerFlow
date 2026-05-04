import {onboardingStyles} from '@/constants/onboardingStyles';
import {COLORS} from '@/constants/theme';
import {MaterialIcons} from '@expo/vector-icons';
import React from 'react';
// Added StyleSheet to the import
import {View} from 'react-native';

export const Step1Illustration = () => (
  <View style={onboardingStyles.illustrationBox}>
    <MaterialIcons name="auto-awesome" size={120} color={COLORS.primary} />
  </View>
);
