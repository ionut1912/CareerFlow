import {MaterialIcons} from '@expo/vector-icons';

type IconName = React.ComponentProps<typeof MaterialIcons>['name'];

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

interface OptionType {
  id: string;
  title: string;
  icon: IconName;
  desc: string;
}

export type {
  ApiErrorResponse,
  RegisterForm,
  TouchedFields,
  ErrorFields,
  OptionType,
};
