import { StyleSheet } from 'react-native';
import { COLORS } from '@/constants/theme';

export const markdownStyles = StyleSheet.create({
  body: { color: COLORS.textSecondary, fontSize: 15, lineHeight: 24 },
  heading3: { color: COLORS.text, fontWeight: '700', marginTop: 20, marginBottom: 10 },
  blockquote: {
    backgroundColor: 'rgba(175, 37, 244, 0.1)',
    borderLeftColor: COLORS.primary,
    borderLeftWidth: 4,
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 8,
    marginVertical: 15,
  },
  table: { borderWidth: 1, borderColor: 'rgba(255,255,255,0.1)', borderRadius: 10, marginVertical: 15 },
  tr: { borderBottomWidth: 1, borderBottomColor: 'rgba(255,255,255,0.1)', flexDirection: 'row' },
  th: { padding: 10, fontWeight: 'bold', color: COLORS.primary },
  td: { padding: 10, color: COLORS.textSecondary },
  bullet_list: { marginVertical: 10 },
  hr: { backgroundColor: 'rgba(255,255,255,0.1)', height: 1, marginVertical: 20 },
  strong: { color: COLORS.text, fontWeight: '700' },
});