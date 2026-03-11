import { Redirect } from 'expo-router';
import React from 'react';
import { useAppSelector } from '@/store/hook';
import { View, ActivityIndicator, StyleSheet } from 'react-native';
import { COLORS } from '@/constants/theme';


export default function Index() {
  const { hasSeenOnboarding, isAppReady } = useAppSelector((state) => state.app);

  if (!isAppReady) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  return <Redirect href={hasSeenOnboarding ? "/(auth)/login" : "/(onboarding)"} />;
}

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
});