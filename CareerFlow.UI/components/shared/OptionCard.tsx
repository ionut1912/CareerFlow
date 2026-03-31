import React from 'react';
import {TouchableOpacity, View, Text, StyleSheet} from 'react-native';
import {MaterialIcons} from '@expo/vector-icons';
import {OptionType} from '@/models/ui.models';
import {COLORS} from '@/constants/theme'; // Asigură-te că calea este corectă

interface OptionCardProps {
  item: OptionType;
  isSelected: boolean;
  onPress: () => void;
  isMulti: boolean;
}

// Extragem culorile cu opacitate pentru a respecta regula linter-ului (no-color-literals)
const OPACITY_COLORS = {
  iconBgDefault: 'rgba(255,255,255,0.05)',
  iconBgSelected: 'rgba(175, 37, 244, 0.15)',
};

export const OptionCard = ({
  item,
  isSelected,
  onPress,
  isMulti,
}: OptionCardProps) => (
  <TouchableOpacity
    style={[styles.card, isSelected && styles.cardSelected]}
    onPress={onPress}
    activeOpacity={0.7}>
    <View
      style={[
        styles.iconContainer,
        isSelected && styles.iconContainerSelected,
      ]}>
      <MaterialIcons
        name={item.icon}
        size={28}
        color={isSelected ? COLORS.primary : COLORS.textMuted}
      />
    </View>
    <View style={styles.cardContent}>
      <Text style={[styles.cardTitle, isSelected && styles.textSelected]}>
        {item.title}
      </Text>
      <Text style={styles.cardDesc}>{item.desc}</Text>
    </View>

    <MaterialIcons
      name={
        isMulti
          ? isSelected
            ? 'check-box'
            : 'check-box-outline-blank'
          : isSelected
            ? 'check-circle'
            : 'radio-button-unchecked'
      }
      size={24}
      color={isSelected ? COLORS.primary : COLORS.border}
      style={styles.checkIcon}
    />
  </TouchableOpacity>
);

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.inputBg,
    borderWidth: 1,
    borderColor: COLORS.border,
    borderRadius: 16,
    padding: 16,
    marginBottom: 16, // Adăugat pentru spațiere între carduri, deoarece gap din optionsContainer nu mai se aplică direct în interior
  },
  cardSelected: {
    backgroundColor: COLORS.primaryWash,
    borderColor: COLORS.primaryBorder,
  },
  iconContainer: {
    width: 50,
    height: 50,
    borderRadius: 12,
    backgroundColor: OPACITY_COLORS.iconBgDefault,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 16,
  },
  iconContainerSelected: {
    backgroundColor: OPACITY_COLORS.iconBgSelected,
  },
  cardContent: {
    flex: 1,
  },
  cardTitle: {
    color: COLORS.text,
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 4,
  },
  textSelected: {
    color: COLORS.primary,
  },
  cardDesc: {
    color: COLORS.textMuted,
    fontSize: 13,
  },
  checkIcon: {
    marginLeft: 10,
  },
});
