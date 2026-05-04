import {onboardingStyles} from '@/constants/onboardingStyles';
import {COLORS} from '@/constants/theme';
import MaterialIcons from '@expo/vector-icons/build/MaterialIcons';
import React from 'react';
import {View, Text, StyleSheet} from 'react-native';

export const Step2Illustration = () => (
  <View style={onboardingStyles.illustrationBox}>
    <View style={styles.timelineNode}>
      <MaterialIcons name="flag" size={28} color={COLORS.primary} />
      <Text style={styles.timelineText}>Nivelul 1:Incepe aventura</Text>
    </View>

    <View style={[styles.timelineNode, styles.questNode]}>
      <MaterialIcons name="auto-awesome" size={28} color={COLORS.primary} />
      <Text style={styles.timelineText}>Chestionar zilnic</Text>
    </View>

    <View style={[styles.timelineNode, styles.streakNode]}>
      <MaterialIcons
        name="local-fire-department"
        size={28}
        color={COLORS.textSecondary}
      />
      <Text style={styles.timelineText}>Ziua 5: mentine progres</Text>
    </View>
  </View>
);

const styles = StyleSheet.create({
  questNode: {
    marginLeft: 40,
    borderColor: COLORS.primary,
    borderWidth: 1,
  },
  streakNode: {
    opacity: 0.5,
  },
  timelineNode: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.surfaceLight,
    padding: 18,
    borderRadius: 16,
    marginBottom: 20,
    width: '100%',
  },
  timelineText: {
    color: COLORS.text,
    marginLeft: 16,
    fontWeight: '600',
    fontSize: 16,
  },
});
