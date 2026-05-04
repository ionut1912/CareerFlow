import {StyleSheet} from 'react-native';
import {COLORS} from './theme';

export const onboardingStyles = StyleSheet.create({
  illustrationBase: {
    width: '100%',
    height: '100%',
  },
  illustrationBox: {
    flex: 1,
    width: '100%',
    borderBottomLeftRadius: 48,
    borderBottomRightRadius: 48,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 30,
    backgroundColor: COLORS.primaryWash,
  },
});
