// ─── Auth ───────────────────────────────────────────────────────────────────
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  accountType: string;         // 'Customer' | 'Seller' | 'Admin'
  storeName?: string;
  address?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface Login2FAResponse {
  userId: string;
  otpToken: string;
  otpIdentifier: string;
  message: string;
}

export interface ValidateOtpRequest {
  userId: string;
  otp: string;
  otpToken: string;
  otpIdentifier: string;
}

export interface ResendOtpRequest {
  otpIdentifier: string;
}

export interface ResetPasswordDTO {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface DecodedToken {
  nameid: string;
  email: string;
  given_name: string;
  family_name: string;
  role: string;
  exp: number;
}