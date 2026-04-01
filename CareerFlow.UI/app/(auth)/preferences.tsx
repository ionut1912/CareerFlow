import React, {useState} from 'react';
import {
  StyleSheet,
  Text,
  View,
  TouchableOpacity,
  ScrollView,
} from 'react-native';
import {useRouter} from 'expo-router';
import {COLORS} from '@/constants/theme';
import {SafeAreaView} from 'react-native-safe-area-context';
import {GradientButton} from '@/components/shared/GradientButton';
import {OptionType} from '@/models/ui.models';
import {OptionCard} from '@/components/shared/OptionCard';
import {showErrorToast, showSuccessToast} from '@/utils/toast';
import {useAppDispatch, useAppSelector} from '@/store/hook';
import {CreateUserProfileRequest} from '@/models/userProfile.models';
import {createUserProfileThunk} from '@/store/userProfile/thunks';

const MOTIVATIONS: OptionType[] = [
  {
    id: 'Student',
    title: 'Student',
    icon: 'school',
    desc: 'Învăț pentru facultate sau școală',
  },
  {
    id: 'JobSearcher',
    title: 'Job / Carieră',
    icon: 'work',
    desc: 'Vreau să mă dezvolt profesional',
  },
  {
    id: 'HobbyLearner',
    title: 'Hobby / Pasiune',
    icon: 'favorite',
    desc: 'Învăț din curiozitate și plăcere',
  },
];

const LEARNING_STYLES: OptionType[] = [
  {
    id: 'Visual',
    title: 'Vizual',
    icon: 'visibility',
    desc: 'Prefer imagini, diagrame și videoclipuri',
  },
  {
    id: 'Auditory',
    title: 'Auditiv',
    icon: 'hearing',
    desc: 'Rețin mai bine ascultând explicații',
  },
  {
    id: 'ReadWrite',
    title: 'Citire/Scriere',
    icon: 'menu-book',
    desc: 'Prefer să citesc și să iau notițe detaliate',
  },
  {
    id: 'Combined',
    title: 'Combinat',
    icon: 'layers',
    desc: 'Nu am un stil preferat, mă adaptez în funcție de conținut',
  },
];

export default function PreferencesScreen() {
  const router = useRouter();

  const [step, setStep] = useState(1);
  const [selectedMotivations, setSelectedMotivations] = useState<string[]>([]);
  const [selectedStyle, setSelectedStyle] = useState<string | null>(null);

  const dispatch = useAppDispatch();
  const {loading} = useAppSelector(state => state.userProfile);

  const toggleMotivation = (id: string) => {
    setSelectedMotivations(prev =>
      prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id],
    );
  };

  const handleNext = () => {
    if (step === 1 && selectedMotivations.length > 0) {
      setStep(2);
    } else if (step === 2 && selectedStyle) {
      handleSavePreferences();
    }
  };

  const handleSavePreferences = async () => {
    try {
      const createUserProfileRequest: CreateUserProfileRequest = {
        learningType: selectedStyle ?? '',
        userTypes: selectedMotivations,
      };
      await dispatch(createUserProfileThunk(createUserProfileRequest)).unwrap();
      // dispatch(yourReduxActionHere(createUserProfileRequest));

      showSuccessToast('Preferințe salvate!', 'Bucură-te de învățare!');
      router.replace('/(tabs)');
    } catch (error) {
      showErrorToast('Eroare la salvarea preferințelor', error);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.header}>
          <Text style={styles.stepIndicator}>Pasul {step} din 2</Text>
          <Text style={styles.title}>
            {step === 1
              ? 'Care este obiectivul tău?'
              : 'Cum preferi să înveți?'}
          </Text>
          <Text style={styles.subtitle}>
            {step === 1
              ? 'Poți selecta mai multe opțiuni care ți se potrivesc.'
              : 'Adaptează formatul cursurilor pentru un randament maxim.'}
          </Text>
        </View>

        <View style={styles.optionsContainer}>
          {step === 1
            ? MOTIVATIONS.map(item => (
                <OptionCard
                  key={item.id}
                  item={item}
                  isMulti={true}
                  isSelected={selectedMotivations.includes(item.id)}
                  onPress={() => toggleMotivation(item.id)}
                />
              ))
            : LEARNING_STYLES.map(item => (
                <OptionCard
                  key={item.id}
                  item={item}
                  isMulti={false}
                  isSelected={selectedStyle === item.id}
                  onPress={() => setSelectedStyle(item.id)}
                />
              ))}
        </View>
      </ScrollView>

      <View style={styles.footer}>
        {step === 2 && (
          <TouchableOpacity
            style={styles.backButton}
            onPress={() => setStep(1)}
            disabled={loading}>
            <Text style={styles.backButtonText}>Înapoi</Text>
          </TouchableOpacity>
        )}

        <View style={styles.nextButtonWrapper}>
          <GradientButton
            text={
              loading
                ? 'Se salvează...'
                : step === 1
                  ? 'Continuă'
                  : 'Finalizare'
            }
            icon={loading ? null : step === 1 ? 'arrow-forward' : 'check'}
            onPress={handleNext}
            disabled={
              (step === 1 && selectedMotivations.length === 0) ||
              (step === 2 && !selectedStyle) ||
              loading
            }
          />
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, backgroundColor: COLORS.background},
  scrollContent: {padding: 24, paddingTop: 40},
  header: {marginBottom: 32},
  stepIndicator: {
    color: COLORS.primary,
    fontSize: 14,
    fontWeight: '700',
    marginBottom: 8,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  title: {
    color: COLORS.text,
    fontSize: 28,
    fontWeight: 'bold',
    marginBottom: 12,
  },
  subtitle: {color: COLORS.textSecondary, fontSize: 15, lineHeight: 22},
  optionsContainer: {gap: 16},
  footer: {
    flexDirection: 'row',
    padding: 24,
    paddingBottom: 40,
    borderTopWidth: 1,
    borderTopColor: COLORS.border,
    backgroundColor: COLORS.background,
    alignItems: 'center',
  },
  backButton: {paddingVertical: 14, paddingHorizontal: 20, marginRight: 16},
  backButtonText: {
    color: COLORS.textSecondary,
    fontSize: 16,
    fontWeight: '600',
  },
  nextButtonWrapper: {flex: 1},
});
