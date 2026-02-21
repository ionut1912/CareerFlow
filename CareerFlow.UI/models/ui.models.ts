interface ApiErrorResponse {
  message: string;
}

interface RegisterForm {
  name: string;
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
}

interface TouchedFields {
  name: boolean;
  email: boolean;
  password: boolean;
  confirmPassword: boolean;
  username: boolean;
}

interface ErrorFields {
  name: string | null;
  email: string | null;
  password: string | null;
  confirmPassword: string | null;
  username: string | null;
}

export type { ApiErrorResponse, RegisterForm, TouchedFields, ErrorFields };