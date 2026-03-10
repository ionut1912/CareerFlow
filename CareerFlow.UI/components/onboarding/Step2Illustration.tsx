import {styles as globalStyles} from '@/constants/onboardingData';
import {COLORS} from '@/constants/theme';
import MaterialIcons from '@expo/vector-icons/build/MaterialIcons';
import React from 'react';
import {View, Text, StyleSheet} from 'react-native';

export const Step2Illustration = () => (
  <View style={globalStyles.illustrationBox}>
    <View style={globalStyles.timelineNode}>
      <MaterialIcons name="flag" size={28} color={COLORS.primary} />
      <Text style={globalStyles.timelineText}>Level 1: Start Journey</Text>
    </View>

    <View style={[globalStyles.timelineNode, localStyles.questNode]}>
      <MaterialIcons name="auto-awesome" size={28} color={COLORS.primary} />
      <Text style={globalStyles.timelineText}>Daily Quest</Text>
    </View>

    <View style={[globalStyles.timelineNode, localStyles.streakNode]}>
      <MaterialIcons
        name="local-fire-department"
        size={28}
        color={COLORS.textSecondary}
      />
      <Text style={globalStyles.timelineText}>Day 5: Maintain Streak</Text>
    </View>
  </View>
);

const localStyles = StyleSheet.create({
  questNode: {
    marginLeft: 40,
    borderColor: COLORS.primary,
    borderWidth: 1,
  },
  streakNode: {
    opacity: 0.5,
  },
});
