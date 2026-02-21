interface CreateAccountRequest {
    email: string;
    password: string;
    confirmPassword: string;
    name: string;
    username: string;
}

interface LoginRequest {
    email: string;
    password: string;
}

interface AccountDto {
    id: string;
    email: string;
    username: string;
    name: string;
    token?: string;
    refreshToken?: string;
    isFounder: boolean;
    privacyPolicyAccepted: boolean;
    termsAccepted: boolean;
}

export type { CreateAccountRequest, LoginRequest, AccountDto };