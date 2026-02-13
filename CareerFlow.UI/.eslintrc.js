module.exports = {
  root: true,
  extends: [
    '@react-native', // Sau '@react-native-community' in functie de versiune
    'prettier', // Adaugă asta la final pentru a dezactiva regulile ESLint care intră în conflict cu Prettier
  ],
  plugins: ['prettier'],
  rules: {
    'prettier/prettier': 'error', // Asta face ca indentarea greșită să fie marcată ca Eroare (roșu)
    'react-native/no-inline-styles': 'warn', // Opțional: avertisment pentru stiluri inline
  },
};
