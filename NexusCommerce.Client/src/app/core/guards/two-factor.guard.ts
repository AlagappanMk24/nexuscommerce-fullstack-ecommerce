import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard to prevent direct access to OTP verification page without a valid 2FA session
 * This is a CRITICAL security measure to prevent bypassing 2FA
 */
export const twoFactorGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // SECURITY CHECK 1: User must NOT be already logged in
  if (auth.isLoggedIn()) {
    console.warn('Security: Already logged in user attempted to access OTP page');
    auth.redirectByRole();
    return false;
  }

  // SECURITY CHECK 2: Valid 2FA session must exist
  if (!auth.is2FASessionActive()) {
    console.warn('Security: No valid 2FA session, redirecting to login');
    router.navigate(['/auth/login']);
    return false;
  }

  // SECURITY CHECK 3: Rate limiting check (optional - can be extended)
  const lastAttempt = localStorage.getItem('otp_last_attempt');
  if (lastAttempt) {
    const timeSinceLastAttempt = Date.now() - parseInt(lastAttempt);
    if (timeSinceLastAttempt < 5000) { // 5 seconds cooldown
      console.warn('Security: Too many OTP page access attempts');
      // Still allow but log for monitoring
    }
  }
  localStorage.setItem('otp_last_attempt', Date.now().toString());

  return true;
};