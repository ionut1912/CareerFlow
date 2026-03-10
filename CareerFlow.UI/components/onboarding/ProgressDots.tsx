import React from 'react';
import {View, StyleSheet} from 'react-native';
import {COLORS} from '@/constants/theme';

interface ProgressDotsProps {
  total: number;
  currentIndex: number;
}

export const ProgressDots: React.FC<ProgressDotsProps> = ({
  total,
  currentIndex,
}) => {
  return (
    <View style={styles.container}>
      {Array.from({length: total}).map((_, index) => {
        const isActive = index === currentIndex;
        return (
          <View
            key={index}
            style={[
              styles.dot,
              isActive ? styles.activeDot : styles.inactiveDot,
            ]}
          />
        );
      })}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginBottom: 32,
  },
  dot: {
    height: 6,
    borderRadius: 3,
  },
  activeDot: {
    width: 32,
    backgroundColor: COLORS.primary,
  },
  inactiveDot: {
    width: 6,
    backgroundColor: COLORS.textMuted || '#cbd5e1',
  },
});
