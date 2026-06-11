import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { decodeBuyonicToken } from '../utils/jwt-claims';
import { StorageService } from './storage.service';
import {
  LoginRequest, Login2FAResponse,
  RegisterRequest, ResetPasswordDTO, DecodedToken,
  ValidateOtpRequest, ResendOtpRequest
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private storage = inject(StorageService);
  private router = inject(Router);
  private apiUrl = `${environment.apiUrl}/auth`;

  // 2FA Session Storage Keys
  private readonly OTP_TOKEN_KEY = 'otp_token';
  private readonly OTP_IDENTIFIER_KEY = 'otp_identifier';
  private readonly USER_ID_KEY = 'temp_user_id';

  /**
   * Register a new user
   */
  register(payload: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, payload);
  }

  /**
   * Login - Step 1: Authenticate and receive 2FA challenge
   */
  login(payload: LoginRequest): Observable<Login2FAResponse> {
    return this.http.post<Login2FAResponse>(`${this.apiUrl}/login`, payload).pipe(
      tap(response => {
        console.log('Login challenge received, 2FA required');
        // Store 2FA session data for OTP verification
        if (response.userId && response.otpToken && response.otpIdentifier) {
          this.storage.setItem(this.OTP_TOKEN_KEY, response.otpToken);
          this.storage.setItem(this.OTP_IDENTIFIER_KEY, response.otpIdentifier);
          this.storage.setItem(this.USER_ID_KEY, response.userId);
        }
      })
    );
  }

  /**
   * Validate OTP and complete login
   */
  validateOtp(otpCode: string): Observable<{ token: string; expiresIn: number }> {
    const userId = this.storage.getItem(this.USER_ID_KEY);
    const otpToken = this.storage.getItem(this.OTP_TOKEN_KEY);
    const otpIdentifier = this.storage.getItem(this.OTP_IDENTIFIER_KEY);

    if (!userId || !otpToken || !otpIdentifier) {
      return throwError(() => new Error('Session expired. Please login again.'));
    }

    const payload: ValidateOtpRequest = {
      userId: userId,
      otp: otpCode,
      otpToken: otpToken,
      otpIdentifier: otpIdentifier
    };

    return this.http.post<{ token: string; expiresIn: number }>(`${this.apiUrl}/validate-otp`, payload).pipe(
      tap(response => {
        console.log('OTP validated successfully, storing JWT token');
        this.storage.setToken(response.token);
        this.clear2FASession();
      })
    );
  }

  /**
   * Resend OTP code
   */
  resendOtp(): Observable<{ message: string }> {
    const otpIdentifier = this.storage.getItem(this.OTP_IDENTIFIER_KEY);

    if (!otpIdentifier) {
      return throwError(() => new Error('Session expired. Please login again.'));
    }

    const payload: ResendOtpRequest = {
      otpIdentifier: otpIdentifier
    };

    return this.http.post<{ message: string }>(`${this.apiUrl}/resend-otp`, payload).pipe(
      tap(response => {
        console.log('OTP resent successfully');
      })
    );
  }

  /**
   * Forgot password - sends reset link to email
   */
  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/forgot-password`,
      JSON.stringify(email),
      {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }
    );
  }

  /**
   * Reset password with token
   */
  resetPassword(payload: ResetPasswordDTO): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, payload);
  }

  /**
   * Logout user
   */
  logout(): void {
    this.storage.removeToken();
    this.clear2FASession();
    this.router.navigate(['/auth/login']);
  }

  /**
   * Get decoded JWT token
   */
  getDecodedToken(): DecodedToken | null {
    const token = this.storage.getToken();
    if (!token) return null;
    return decodeBuyonicToken(token);
  }

  /**
   * Get user role from token
   */
  getRole(): string | null {
    const decoded = this.getDecodedToken();
    return decoded?.role || null;
  }

  /**
   * Get user ID from token
   */
  getUserId(): string | null {
    const decoded = this.getDecodedToken();
    return decoded?.nameid || null;
  }

  /**
   * Get user email from token
   */
  getEmail(): string | null {
    const decoded = this.getDecodedToken();
    return decoded?.email || null;
  }

  /**
   * Get user full name from token
   */
  getFullName(): string {
    const decoded = this.getDecodedToken();
    if (!decoded) return '';
    return `${decoded.given_name} ${decoded.family_name}`;
  }

  /**
   * Check if user is logged in
   */
  isLoggedIn(): boolean {
    const token = this.storage.getToken();
    if (!token) return false;
    const decoded = decodeBuyonicToken(token);
    if (!decoded) return false;
    return decoded.exp * 1000 > Date.now();
  }

  /**
   * Check if 2FA session is active
   */
  is2FASessionActive(): boolean {
    return !!(
      this.storage.getItem(this.OTP_TOKEN_KEY) &&
      this.storage.getItem(this.OTP_IDENTIFIER_KEY) &&
      this.storage.getItem(this.USER_ID_KEY)
    );
  }

  /**
   * Get OTP identifier for resend
   */
  getOtpIdentifier(): string | null {
    return this.storage.getItem(this.OTP_IDENTIFIER_KEY);
  }

  /**
   * Redirect user based on role
   */
  redirectByRole(): void {
    const role = this.getRole();
    switch (role) {
      case 'Customer':
        this.router.navigate(['/customer']);
        break;
      case 'Seller':
        this.router.navigate(['/seller']);
        break;
      case 'Admin':
        this.router.navigate(['/admin/dashboard']);
        break;
      default:
        this.router.navigate(['/auth/login']);
        break;
    }
  }

  /**
   * Clear 2FA session data
   */
  private clear2FASession(): void {
    this.storage.removeItem(this.OTP_TOKEN_KEY);
    this.storage.removeItem(this.OTP_IDENTIFIER_KEY);
    this.storage.removeItem(this.USER_ID_KEY);
  }
}