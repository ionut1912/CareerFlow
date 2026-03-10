import {styles} from '@/constants/onboardingData';
import {COLORS} from '@/constants/theme';
import {MaterialIcons} from '@expo/vector-icons';
import React from 'react';
import {View} from 'react-native';

export const Step1Illustration = () => (
  <View style={styles.illustrationBox}>
    <MaterialIcons name="auto-awesome" size={120} color={COLORS.primary} />
  </View>
);
