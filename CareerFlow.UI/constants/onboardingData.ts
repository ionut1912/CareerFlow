import {Step1Illustration} from '@/components/onboarding/Step1Illustration';
import {Step2Illustration} from '@/components/onboarding/Step2Illustration';
import {Step3Illustration} from '@/components/onboarding/Step3Ilustration';
import {COLORS} from './theme';
import {StyleSheet} from 'react-native';

export interface OnboardingStepProps {
  id: string;
  title: string;
  subtitle: string;
  Illustration: React.ComponentType;
}

// 2. Replaced all raw strings with COLORS references
export const styles = StyleSheet.create({
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

  // --- STEP 2 STYLES ---
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

  // --- STEP 3 STYLES ---
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

export const ONBOARDING_STEPS: OnboardingStepProps[] = [
  {
    id: '1',
    title: 'Invatare personalizata',
    subtitle:
      'Stapaneste concepte noi cu ajutorul explicatiilor clare si ilustratiilor captivante, adaptate stilului tau de invatare.',
    Illustration: Step1Illustration,
  },
  {
    id: '2',
    title: 'Timeline Interactiv',
    subtitle:
      'Urmareste calea ta unica de invatare, castiga XP si pastreaza seriele tale pe masura ce progresezi.',
    Illustration: Step2Illustration,
  },
  {
    id: '3',
    title: 'Navigare Usoara',
    subtitle:
      'Foloseste meniul intuitiv pentru a accesa rapid lectiile, resursele si progresul tau, pastrand totul organizat intr-un singur loc.',
    Illustration: Step3Illustration,
  },
];
