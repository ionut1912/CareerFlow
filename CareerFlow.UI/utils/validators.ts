export const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const validateEmail = (value: string): string | null => {
  if (!value) return 'Email-ul este necesar';
  if (!EMAIL_REGEX.test(value)) return 'Format invalid';
  return null;
};

export const validatePassword = (value: string): string | null => {
  if (!value) return 'Parola este necesara';
  if (value.length < 6) return 'Parola trebuie sa aiba minim 6 caractere';
  return null;
};

export const validateRequired = (
  value: string,
  message: string,
): string | null => (value.trim() ? null : message);

export const validateConfirmPassword = (
  password: string,
  confirmPassword: string,
): string | null => {
  if (!confirmPassword) return 'Confirmarea parolei este necesara';
  if (confirmPassword !== password) return 'Parolele nu se potrivesc';
  return null;
};

export const validateName = (value: string): string | null => {
  if (!value) return 'Numele este necesar';
  if (value.trim().length < 2)
    return 'Numele trebuie sa aiba minim 2 caractere';
  return null;
};
