import type {ComponentType} from 'react';
import {Step1Illustration} from '@/components/onboarding/Step1Illustration';
import {Step2Illustration} from '@/components/onboarding/Step2Illustration';
import {Step3Illustration} from '@/components/onboarding/Step3Ilustration';

export interface OnboardingStepProps {
  id: string;
  title: string;
  subtitle: string;
  Illustration: ComponentType;
}

export const ONBOARDING_STEPS: OnboardingStepProps[] = [
  {
    id: '1',
    title: 'Bun venit',
    subtitle: 'Descopera aplicatia noastra',
    Illustration: Step1Illustration,
  },
  {
    id: '2',
    title: 'Invata',
    subtitle: 'Creste-ti cunostintele',
    Illustration: Step2Illustration,
  },
  {
    id: '3',
    title: 'Succces',
    subtitle: 'Indeplineste obiectivele',
    Illustration: Step3Illustration,
  },
];
