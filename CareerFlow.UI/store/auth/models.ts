import {AccountDto} from '@/models/auth.models';

export interface AuthState {
  account: AccountDto | null | undefined;
  loading: boolean;
  error: string | null;
}
