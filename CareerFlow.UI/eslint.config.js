const expoConfig = require('eslint-config-expo/flat');
const prettierPlugin = require('eslint-plugin-prettier');
const prettierConfig = require('eslint-config-prettier');
const reactNativePlugin = require('eslint-plugin-react-native');
const tsPlugin = require('@typescript-eslint/eslint-plugin');
const tsParser = require('@typescript-eslint/parser');

module.exports = [
  ...expoConfig,
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      prettier: prettierPlugin,
      'react-native': reactNativePlugin,
      '@typescript-eslint': tsPlugin,
    },
    languageOptions: {
      parser: tsParser,
      parserOptions: {
        ecmaFeatures: { jsx: true },
      },
    },
    rules: {
      ...prettierConfig.rules,
      'prettier/prettier': 'error',
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-empty-object-type': 'error',
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'error',
      'react/jsx-no-useless-fragment': 'error',
      'react/jsx-key': 'error',
      'react-native/no-inline-styles': 'error',
      'react-native/no-unused-styles': 'error',
      'react-native/no-color-literals': 'error',
      'react-native/split-platform-components': 'error',
      'no-console': ['error', { allow: ['warn', 'error'] }],
      'eqeqeq': 'error',
      'prefer-const': 'error',
      'no-var': 'error',
    },
  },
  {
    ignores: ['dist/*', '.expo/*', 'web-build/*', 'node_modules/*', 'babel.config.js', 'metro.config.js'],
  },
];