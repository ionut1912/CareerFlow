import React, {useState} from 'react';
import {
  StyleSheet,
  Text,
  View,
  TouchableOpacity,
  ScrollView,
  SafeAreaView,
} from 'react-native';
import {useRouter} from 'expo-router';
import {COLORS} from '@/constants/theme';
import {GradientButton} from '@/components/shared/GradientButton';
import {OptionType} from '@/models/ui.models';
import {OptionCard} from '@/components/shared/OptionCard';

// Configurarea opțiunilor cu tipare strictă
const MOTIVATIONS: OptionType[] = [
  {
    id: 'student',
    title: 'Student',
    icon: 'school',
    desc: 'Învăț pentru facultate sau școală',
  },
  {
    id: 'job',
    title: 'Job / Carieră',
    icon: 'work',
    desc: 'Vreau să mă dezvolt profesional',
  },
  {
    id: 'hobby',
    title: 'Hobby / Pasiune',
    icon: 'favorite',
    desc: 'Învăț din curiozitate și plăcere',
  },
];

const LEARNING_STYLES: OptionType[] = [
  {
    id: 'visual',
    title: 'Vizual',
    icon: 'visibility',
    desc: 'Prefer imagini, diagrame și videoclipuri',
  },
  {
    id: 'auditory',
    title: 'Auditiv',
    icon: 'hearing',
    desc: 'Rețin mai bine ascultând explicații',
  },
  {
    id: 'kinesthetic',
    title: 'Kinestezic',
    icon: 'touch-app',
    desc: 'Învăț cel mai bine făcând și practicând',
  },
  {
    id: 'unknown',
    title: 'Nu știu încă',
    icon: 'help-outline',
    desc: 'Ajută-mă să descopăr pe parcurs',
  },
];

export default function PreferencesScreen() {
  const router = useRouter();

  const [step, setStep] = useState(1);
  const [selectedMotivations, setSelectedMotivations] = useState<string[]>([]);
  const [selectedStyle, setSelectedStyle] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

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
    setIsLoading(true);
    try {
      // await dispatch(updateUserPreferences({ motivations: selectedMotivations, style: selectedStyle })).unwrap();
      router.replace('/(tabs)');
    } catch (error) {
      console.error(error);
    } finally {
      setIsLoading(false);
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
            disabled={isLoading}>
            <Text style={styles.backButtonText}>Înapoi</Text>
          </TouchableOpacity>
        )}

        <View style={styles.nextButtonWrapper}>
          <GradientButton
            text={
              isLoading
                ? 'Se salvează...'
                : step === 1
                  ? 'Continuă'
                  : 'Finalizare'
            }
            icon={isLoading ? null : step === 1 ? 'arrow-forward' : 'check'}
            onPress={handleNext}
            disabled={
              (step === 1 && selectedMotivations.length === 0) ||
              (step === 2 && !selectedStyle) ||
              isLoading
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
