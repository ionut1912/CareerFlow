import React from 'react';
import { TouchableOpacity, Text, StyleSheet } from 'react-native';
import { COLORS } from '@/constants/theme';

interface TabButtonProps {
  title: string;
  active: boolean;
  onPress: () => void;
}

export const TabButton: React.FC<TabButtonProps> = ({ title, active, onPress }) => (
  <TouchableOpacity style={[styles.tab, active && styles.activeTab]} onPress={onPress}>
    <Text style={[styles.tabText, active && styles.activeTabText]}>{title}</Text>
  </TouchableOpacity>
);

const styles = StyleSheet.create({
  tab: { flex: 1, paddingVertical: 10, alignItems: 'center', borderRadius: 8 },
  activeTab: { backgroundColor: COLORS.primary },
  tabText: { color: COLORS.textSecondary, fontSize: 14, fontWeight: '600' },
  activeTabText: { color: COLORS.text },
});