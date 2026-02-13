import {FontAwesome} from '@expo/vector-icons';
import React from 'react';
import {StyleSheet, Text, TouchableOpacity, View} from 'react-native';

export default function SocialLoginButtons() {
  return (
    <View className="space-y-3 mt-6">
      {/* Divider */}
      <TouchableOpacity style={styles.socialBtn}>
        <FontAwesome
          name="google"
          size={20}
          color="white"
          style={{marginRight: 10}}
        />
        <Text style={styles.socialBtnText}>Google</Text>
      </TouchableOpacity>
      <TouchableOpacity style={[styles.socialBtn, {marginTop: 12}]}>
        <FontAwesome
          name="linkedin-square"
          size={20}
          color="white"
          style={{marginRight: 10}}
        />
        <Text style={styles.socialBtnText}>LinkedIn</Text>
      </TouchableOpacity>
    </View>
  );
}
const styles = StyleSheet.create({
  socialBtn: {
    flexDirection: 'row',
    height: 52,
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
  },
  socialBtnText: {
    color: '#e5e7eb',
    fontSize: 14,
    fontWeight: '600',
  },
});
