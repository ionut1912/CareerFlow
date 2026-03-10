import {COLORS} from '@/constants/theme';
import MaterialIcons from '@expo/vector-icons/build/MaterialIcons';
import {onboardingStyles} from '@/constants/onboardingStyles';
import React from 'react';
import {View, Text, StyleSheet} from 'react-native';

export const Step3Illustration = () => (
  <View style={onboardingStyles.illustrationBox}>
    {/* Wrapped the cards in a container to put them side-by-side */}
    <View style={styles.mockupContainer}>
      <View style={styles.mockupCard}>
        <MaterialIcons name="psychology" size={40} color={COLORS.primary} />
        <Text style={styles.mockupText}>Mentor personal</Text>
      </View>
      <View style={styles.mockupCard}>
        <MaterialIcons name="analytics" size={40} color={COLORS.primary} />
        <Text style={styles.mockupText}>Monitorizeaza cresterea</Text>
      </View>
    </View>
  </View>
);

const styles = StyleSheet.create({
  mockupContainer: {
    flexDirection: 'row',
    gap: 16,
    width: '100%',
    justifyContent: 'center',
  },
  mockupCard: {
    backgroundColor: COLORS.surfaceLight,
    padding: 24,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
    flex: 1,
    aspectRatio: 1,
    borderWidth: 1,
    borderColor: COLORS.primaryBorder,
  },
  mockupText: {
    color: COLORS.text,
    marginTop: 12,
    fontWeight: 'bold',
    fontSize: 15,
    textAlign: 'center',
  },
});
