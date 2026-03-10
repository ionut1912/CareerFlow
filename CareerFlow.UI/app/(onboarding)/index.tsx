import React, {useState} from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  Dimensions,
  NativeScrollEvent,
  NativeSyntheticEvent,
} from 'react-native';
import {useRouter} from 'expo-router';
import {COLORS} from '@/constants/theme';
import {GradientButton} from '@/components/shared/GradientButton';
import {ProgressDots} from '@/components/onboarding/ProgressDots';
import {ONBOARDING_STEPS} from '@/constants/onboardingData';
import {useAppDispatch} from '@/store/hook';
import {completeOnboardingThunk} from '@/store/app/thunks';
import {SafeAreaView} from 'react-native-safe-area-context';

const {width} = Dimensions.get('window');

export default function OnboardingScreen() {
  const router = useRouter();
  const dispatch = useAppDispatch();
  const [currentIndex, setCurrentIndex] = useState(0);

  const isLastStep = currentIndex === ONBOARDING_STEPS.length - 1;

  const handleScroll = (event: NativeSyntheticEvent<NativeScrollEvent>) => {
    const contentOffset = event.nativeEvent.contentOffset.x;
    const index = Math.round(contentOffset / width);
    if (index !== currentIndex) {
      setCurrentIndex(index);
    }
  };

  const finishOnboarding = async () => {
    await dispatch(completeOnboardingThunk()).unwrap();
    router.replace('/(auth)/login');
  };

  const renderItem = ({item}: {item: (typeof ONBOARDING_STEPS)[0]}) => (
    <View style={styles.slide}>
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

      <View style={styles.footer}>
        <ProgressDots
          total={ONBOARDING_STEPS.length}
          currentIndex={currentIndex}
        />

        <View style={styles.actionArea}>
          {isLastStep ? (
            <GradientButton
              text="Get Started"
              icon="rocket"
              onPress={finishOnboarding}
            />
          ) : (
            <Text style={styles.swipeHint}>Swipe to continue</Text>
          )}
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background, // Removed the fallback literal
  },
  flatList: {
    flex: 1,
  },
  slide: {
    width: width,
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
    color: COLORS.text, // Replaced #FFFFFF
    textAlign: 'center',
    marginBottom: 16,
    letterSpacing: -0.5,
  },
  subtitle: {
    fontSize: 17,
    color: COLORS.textSecondary, // Replaced #cbd5e1
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
    color: COLORS.textMuted, // Replaced #64748b
    textAlign: 'center',
    fontSize: 14,
    fontWeight: '500',
    opacity: 0.8,
  },
});
