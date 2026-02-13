const expoConfig = require('eslint-config-expo/flat');
const prettierPlugin = require('eslint-plugin-prettier');
const prettierConfig = require('eslint-config-prettier');
const reactNativePlugin = require('eslint-plugin-react-native');

module.exports = [
  // 1. Base Expo Configuration
  ...expoConfig,

  // 2. Prettier & React Native Configuration
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    plugins: {
      prettier: prettierPlugin,
      'react-native': reactNativePlugin,
    },
    // Apply Prettier's config to disable conflicting ESLint formatting rules
    rules: {
      ...prettierConfig.rules,

      // --- Prettier Integration ---
      'prettier/prettier': 'error',

      // --- React Native Best Practices ---
      // Warns if you write styles directly in the JSX (performance & maintainability)
      'react-native/no-inline-styles': 'error',

      // Errors if you define a style in StyleSheet but never use it
      'react-native/no-unused-styles': 'error',

      // (Optional) Encourages using theme colors/constants instead of raw hex codes
      'react-native/no-color-literals': 'off',

      // (Optional) Ensures styling arrays are flattened (optimization)
      'react-native/no-single-element-style-arrays': 'warn',
    },
    // Settings required for eslint-plugin-react-native to detect components
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
      },
    },
  },

  // 3. Global Ignores
  {
    ignores: ['dist/*', '.expo/*', 'web-build/*', 'node_modules/*'],
  },
];
