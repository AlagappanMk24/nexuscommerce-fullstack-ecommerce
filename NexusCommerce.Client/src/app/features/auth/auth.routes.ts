import { Routes } from '@angular/router';
import { twoFactorGuard } from '../../core/guards/two-factor.guard';

export const authRoutes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'verify-otp',
    loadComponent: () =>
      import('./verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./reset-password/reset-password.component').then((m) => m.ResetPasswordComponent),
  },
   // SECURITY: OTP verification route - protected by twoFactorGuard
  // Users cannot access this directly without a valid 2FA session
  {
    path: 'verify-otp',
    loadComponent: () => import('./verify-otp/verify-otp.component').then(m => m.VerifyOtpComponent),
    canActivate: [twoFactorGuard]  // CRITICAL: Guard prevents direct access
  }
];
