import {styles} from '@/constants/onboardingData';
import {COLORS} from '@/constants/theme';
import MaterialIcons from '@expo/vector-icons/build/MaterialIcons';
import React from 'react';
import {View, Text} from 'react-native';

export const Step3Illustration = () => (
  <View style={styles.illustrationBox}>
    {/* Wrapped the cards in a container to put them side-by-side */}
    <View style={styles.mockupContainer}>
      <View style={styles.mockupCard}>
        <MaterialIcons name="psychology" size={40} color={COLORS.primary} />
        <Text style={styles.mockupText}>AI Mentor</Text>
      </View>
      <View style={styles.mockupCard}>
        <MaterialIcons name="analytics" size={40} color={COLORS.primary} />
        <Text style={styles.mockupText}>Track Growth</Text>
      </View>
    </View>
  </View>
);
