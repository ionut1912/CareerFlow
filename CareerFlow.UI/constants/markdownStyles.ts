import {StyleSheet} from 'react-native';
import {COLORS} from '@/constants/theme';

const MARKDOWN_COLORS = {
  blockquoteBg: 'rgba(175, 37, 244, 0.1)',
  subtle: 'rgba(255,255,255,0.1)',
};

export const markdownStyles = StyleSheet.create({
  body: {color: COLORS.textSecondary, fontSize: 15, lineHeight: 24},
  heading3: {
    color: COLORS.text,
    fontWeight: '700',
    marginTop: 20,
    marginBottom: 10,
  },
  blockquote: {
    backgroundColor: MARKDOWN_COLORS.blockquoteBg,
    borderLeftColor: COLORS.primary,
    borderLeftWidth: 4,
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 8,
    marginVertical: 15,
  },
  table: {
    borderWidth: 1,
    borderColor: MARKDOWN_COLORS.subtle,
    borderRadius: 10,
    marginVertical: 15,
  },
  tr: {
    borderBottomWidth: 1,
    borderBottomColor: MARKDOWN_COLORS.subtle,
    flexDirection: 'row',
  },
  th: {padding: 10, fontWeight: 'bold', color: COLORS.primary},
  td: {padding: 10, color: COLORS.textSecondary},
  bullet_list: {marginVertical: 10},
  hr: {backgroundColor: MARKDOWN_COLORS.subtle, height: 1, marginVertical: 20},
  strong: {color: COLORS.text, fontWeight: '700'},
});
