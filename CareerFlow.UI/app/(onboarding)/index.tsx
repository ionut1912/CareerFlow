import React, {useState, useEffect} from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  useWindowDimensions,
  NativeScrollEvent,
  NativeSyntheticEvent,
  AccessibilityInfo,
} from 'react-native';
import {useRouter} from 'expo-router';
import {COLORS} from '@/constants/theme';
import {GradientButton} from '@/components/shared/GradientButton';
import {ProgressDots} from '@/components/onboarding/ProgressDots';
import {ONBOARDING_STEPS} from '@/constants/onboardingData';
import {useAppDispatch} from '@/store/hook';
import {completeOnboardingThunk} from '@/store/app/thunks';
import {SafeAreaView} from 'react-native-safe-area-context';
import {showErrorToast} from '@/utils/toast';

export default function OnboardingScreen() {
  const {width} = useWindowDimensions();
  const router = useRouter();
  const dispatch = useAppDispatch();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const isLastStep = currentIndex === ONBOARDING_STEPS.length - 1;

  useEffect(() => {
    const message = `Pasul ${currentIndex + 1} din ${ONBOARDING_STEPS.length}`;
    AccessibilityInfo.announceForAccessibility(message);
  }, [currentIndex]);

  const handleScroll = (event: NativeSyntheticEvent<NativeScrollEvent>) => {
    const contentOffset = event.nativeEvent.contentOffset.x;
    const index = Math.round(contentOffset / width);
    if (index !== currentIndex) {
      setCurrentIndex(index);
    }
  };

  const finishOnboarding = async () => {
    if (isLoading) return;
    setIsLoading(true);
    try {
      await dispatch(completeOnboardingThunk()).unwrap();
      router.replace('/(auth)/login');
    } catch (error) {
      setIsLoading(false);
      showErrorToast('Eroare la finalizarea onboarding-ului', error);
    }
  };

  const renderItem = ({item}: {item: (typeof ONBOARDING_STEPS)[0]}) => (
    <View style={[styles.slide, {width}]}>
      <View style={styles.illustrationContainer}>
        <item.Illustration width="100%" height="100%" />
      </View>
      <View style={styles.textContainer}>
        <Text style={styles.title}>{item.title}</Text>
        <Text style={styles.subtitle}>{item.subtitle}</Text>
      </View>
    </View>
  );

  return (
    <SafeAreaView style={styles.container} edges={['top', 'bottom']}>
      <FlatList
        testID="onboarding-list"
        data={ONBOARDING_STEPS}
        renderItem={renderItem}
        horizontal
        pagingEnabled
        bounces={false}
        showsHorizontalScrollIndicator={false}
        onScroll={handleScroll}
        keyExtractor={(_, index) => index.toString()}
        scrollEventThrottle={16}
        style={styles.flatList}
      />

      <View style={styles.footer} accessibilityLiveRegion="polite">
        <Text style={styles.srOnly}>
          {`Pasul ${currentIndex + 1} din ${ONBOARDING_STEPS.length}`}
        </Text>

        <ProgressDots
          total={ONBOARDING_STEPS.length}
          currentIndex={currentIndex}
        />

        <View style={styles.actionArea}>
          {isLastStep ? (
            <GradientButton
              text={isLoading ? 'Se incarca...' : 'Get Started'}
              icon="rocket"
              onPress={finishOnboarding}
              disabled={isLoading}
            />
          ) : (
            <Text style={styles.swipeHint}>Gliseaza pentru a continua</Text>
          )}
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  flatList: {
    flex: 1,
  },
  slide: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  illustrationContainer: {
    flex: 0.6,
    width: '100%',
    justifyContent: 'center',
    alignItems: 'center',
  },
  textContainer: {
    flex: 0.4,
    alignItems: 'center',
    width: '100%',
    paddingHorizontal: 32,
    paddingTop: 20,
  },
  title: {
    fontSize: 32,
    fontWeight: '900',
    color: COLORS.text,
    textAlign: 'center',
    marginBottom: 16,
    letterSpacing: -0.5,
  },
  subtitle: {
    fontSize: 17,
    color: COLORS.textSecondary,
    textAlign: 'center',
    lineHeight: 26,
  },
  footer: {
    paddingBottom: 40,
    paddingTop: 20,
    alignItems: 'center',
  },
  actionArea: {
    height: 60,
    marginTop: 24,
    width: '100%',
    paddingHorizontal: 32,
    justifyContent: 'center',
  },
  swipeHint: {
    color: COLORS.textMuted,
    textAlign: 'center',
    fontSize: 14,
    fontWeight: '500',
    opacity: 0.8,
  },
  srOnly: {
    position: 'absolute',
    width: 1,
    height: 1,
    padding: 0,
    margin: -1,
    overflow: 'hidden',
    opacity: 0,
  },
});
