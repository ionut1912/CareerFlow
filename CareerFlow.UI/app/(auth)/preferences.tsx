import React, {useState, useCallback} from 'react';
import {StyleSheet, Text, View, TouchableOpacity} from 'react-native';
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
import {FlashList} from '@shopify/flash-list'; // Import FlashList

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

const STEP_TITLES = ['Care este obiectivul tău?', 'Cum preferi să înveți?'];

const STEP_SUBTITLES = [
  'Poți selecta mai multe opțiuni care ți se potrivesc.',
  'Adaptează formatul cursurilor pentru un randament maxim.',
];

export default function PreferencesScreen() {
  const router = useRouter();

  const [step, setStep] = useState(1);
  const [selectedMotivations, setSelectedMotivations] = useState<string[]>([]);
  const [selectedStyle, setSelectedStyle] = useState<string | null>(null);

  const dispatch = useAppDispatch();
  const {loading} = useAppSelector(state => state.userProfile);

  const handleToggleMotivation = useCallback((id: string) => {
    setSelectedMotivations(prev =>
      prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id],
    );
  }, []);

  const handleSelectStyle = useCallback((id: string) => {
    setSelectedStyle(id);
  }, []);

  const handleSavePreferences = useCallback(async () => {
    try {
      const createUserProfileRequest: CreateUserProfileRequest = {
        learningType: selectedStyle ?? '',
        userTypes: selectedMotivations,
      };
      await dispatch(createUserProfileThunk(createUserProfileRequest)).unwrap();

      showSuccessToast('Preferințe salvate!', 'Bucură-te de învățare!');
      router.replace('/(tabs)');
    } catch (error) {
      showErrorToast('Eroare la salvarea preferințelor', error);
    }
  }, [selectedStyle, selectedMotivations, dispatch, router]);

  const handleNext = useCallback(() => {
    if (step === 1 && selectedMotivations.length > 0) {
      setStep(2);
    } else if (step === 2 && selectedStyle) {
      handleSavePreferences();
    }
  }, [step, selectedMotivations.length, selectedStyle, handleSavePreferences]);

  // Memoized renderItem for FlashList performance
  const renderOptionCard = useCallback(
    ({item}: {item: OptionType}) => {
      const isMulti = step === 1;
      const isSelected = isMulti
        ? selectedMotivations.includes(item.id)
        : selectedStyle === item.id;

      return (
        <OptionCard
          item={item}
          isMulti={isMulti}
          isSelected={isSelected}
          onPress={() =>
            isMulti
              ? handleToggleMotivation(item.id)
              : handleSelectStyle(item.id)
          }
        />
      );
    },
    [
      step,
      selectedMotivations,
      selectedStyle,
      handleToggleMotivation,
      handleSelectStyle,
    ],
  );

  const headerA11yLabel = `Pasul ${step} din 2. ${STEP_TITLES[step - 1]}. ${STEP_SUBTITLES[step - 1]}`;

  return (
    <SafeAreaView style={styles.container}>
      <FlashList
        data={step === 1 ? MOTIVATIONS : LEARNING_STYLES}
        renderItem={renderOptionCard}
        keyExtractor={item => item.id}
        estimatedItemSize={84} // Estimated height of OptionCard + marginBottom
        contentContainerStyle={styles.scrollContent}
        // extraData tells FlashList to re-render items when these state values change
        extraData={{step, selectedMotivations, selectedStyle}}
        ListHeaderComponent={
          <View
            style={styles.header}
            accessibilityLiveRegion="polite"
            accessible={true}
            accessibilityLabel={headerA11yLabel}>
            <Text
              style={styles.stepIndicator}
              accessibilityElementsHidden={true}>
              Pasul {step} din 2
            </Text>
            <Text style={styles.title} accessibilityElementsHidden={true}>
              {STEP_TITLES[step - 1]}
            </Text>
            <Text style={styles.subtitle} accessibilityElementsHidden={true}>
              {STEP_SUBTITLES[step - 1]}
            </Text>
          </View>
        }
      />

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
