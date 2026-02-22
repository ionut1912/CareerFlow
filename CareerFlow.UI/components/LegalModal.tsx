import React from 'react';
import {
  Modal,
  View,
  Text,
  TouchableOpacity,
  ScrollView,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import {MaterialIcons} from '@expo/vector-icons';
import Markdown from 'react-native-markdown-display';
import {COLORS} from '@/constants/theme';
import {markdownStyles} from '@/constants/markdownStyles';

interface LegalModalProps {
  visible: boolean;
  loading: boolean;
  title: string;
  content: string;
  onClose: () => void;
  onAccept: () => void;
  onReject: () => void;
}

const MODAL_COLORS = {
  overlay: 'rgba(0,0,0,0.85)',
  background: '#121212',
  divider: 'rgba(255,255,255,0.1)',
  rejectBg: 'rgba(255,255,255,0.05)',
  white: '#fff',
};

export const LegalModal: React.FC<LegalModalProps> = ({
  visible,
  loading,
  title,
  content,
  onClose,
  onAccept,
  onReject,
}) => (
  <Modal
    animationType="slide"
    transparent
    visible={visible}
    onRequestClose={onClose}>
    <View style={styles.overlay}>
      <View style={styles.content}>
        <View style={styles.header}>
          <Text style={styles.title}>{title}</Text>
          <TouchableOpacity onPress={onClose} style={styles.closeIcon}>
            <MaterialIcons name="close" size={24} color={COLORS.text} />
          </TouchableOpacity>
        </View>

        {loading ? (
          <View style={styles.loader}>
            <ActivityIndicator size="large" color={COLORS.primary} />
          </View>
        ) : (
          <>
            <ScrollView
              showsVerticalScrollIndicator={false}
              contentContainerStyle={styles.scrollPadding}>
              <Markdown style={markdownStyles}>{content}</Markdown>
            </ScrollView>

            <View style={styles.actionRow}>
              <TouchableOpacity
                style={[styles.btn, styles.btnReject]}
                onPress={onReject}>
                <Text style={styles.btnTextReject}>Refuză</Text>
              </TouchableOpacity>

              <TouchableOpacity
                style={[styles.btn, styles.btnAccept]}
                onPress={onAccept}>
                <Text style={styles.btnTextAccept}>Acceptă</Text>
              </TouchableOpacity>
            </View>
          </>
        )}
      </View>
    </View>
  </Modal>
);

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: MODAL_COLORS.overlay,
    justifyContent: 'flex-end',
  },
  content: {
    backgroundColor: MODAL_COLORS.background,
    borderTopLeftRadius: 30,
    borderTopRightRadius: 30,
    padding: 24,
    height: '85%',
    borderWidth: 1,
    borderColor: COLORS.border,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 20,
    paddingBottom: 15,
    borderBottomWidth: 1,
    borderBottomColor: MODAL_COLORS.divider,
  },
  title: {fontSize: 18, fontWeight: '800', color: COLORS.text},
  closeIcon: {padding: 4},
  loader: {flex: 1, justifyContent: 'center', alignItems: 'center'},
  scrollPadding: {paddingBottom: 20},
  actionRow: {
    flexDirection: 'row',
    gap: 12,
    paddingTop: 20,
    borderTopWidth: 1,
    borderTopColor: MODAL_COLORS.divider,
  },
  btn: {
    flex: 1,
    paddingVertical: 14,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  btnReject: {
    backgroundColor: MODAL_COLORS.rejectBg,
    borderWidth: 1,
    borderColor: COLORS.border,
  },
  btnAccept: {
    backgroundColor: COLORS.primary,
  },
  btnTextReject: {
    color: COLORS.textSecondary,
    fontWeight: '600',
    fontSize: 14,
  },
  btnTextAccept: {
    color: MODAL_COLORS.white,
    fontWeight: '700',
    fontSize: 14,
  },
});
