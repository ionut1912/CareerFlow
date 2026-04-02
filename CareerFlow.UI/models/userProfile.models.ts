export interface CreateUserProfileRequest {
  learningType: string;
  userTypes: string[];
  /** Defaults to "Student" if not provided */
  domain?: string;
}
