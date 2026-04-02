import React from 'react';
import {TouchableOpacity, View, Text, StyleSheet} from 'react-native';
import {MaterialIcons} from '@expo/vector-icons';
import {OptionType} from '@/models/ui.models';
import {COLORS} from '@/constants/theme';

interface OptionCardProps {
  item: OptionType;
  isSelected: boolean;
  onPress: () => void;
  isMulti: boolean;
}

const OPACITY_COLORS = {
  iconBgDefault: 'rgba(255,255,255,0.05)',
  iconBgSelected: 'rgba(175, 37, 244, 0.15)',
};

const OptionCardComponent = ({
  item,
  isSelected,
  onPress,
  isMulti,
}: OptionCardProps) => (
  <TouchableOpacity
    style={[styles.card, isSelected && styles.cardSelected]}
    onPress={onPress}
    activeOpacity={0.7}
    accessibilityRole={isMulti ? 'checkbox' : 'radio'}
    accessibilityState={{checked: isSelected}}
    accessibilityLabel={`${item.title}: ${item.desc}`}
    accessibilityHint={
      isMulti
        ? isSelected
          ? 'Deselectează această opțiune'
          : 'Selectează această opțiune'
        : 'Alege acest stil de învățare'
    }
    importantForAccessibility="yes">
    <View
      style={[styles.iconContainer, isSelected && styles.iconContainerSelected]}
      importantForAccessibility="no-hide-descendants">
      <MaterialIcons
        name={item.icon}
        size={28}
        color={isSelected ? COLORS.primary : COLORS.textMuted}
      />
    </View>
    <View
      style={styles.cardContent}
      importantForAccessibility="no-hide-descendants">
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
      importantForAccessibility="no"
    />
  </TouchableOpacity>
);

const areEqual = (prev: OptionCardProps, next: OptionCardProps): boolean =>
  prev.isSelected === next.isSelected &&
  prev.isMulti === next.isMulti &&
  prev.item.id === next.item.id;

export const OptionCard = React.memo(OptionCardComponent, areEqual);

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.inputBg,
    borderWidth: 1,
    borderColor: COLORS.border,
    borderRadius: 16,
    padding: 16,
    marginBottom: 16,
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
  iconContainerSelected: {backgroundColor: OPACITY_COLORS.iconBgSelected},
  cardContent: {flex: 1},
  cardTitle: {
    color: COLORS.text,
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 4,
  },
  textSelected: {color: COLORS.primary},
  cardDesc: {color: COLORS.textMuted, fontSize: 13},
  checkIcon: {marginLeft: 10},
});
