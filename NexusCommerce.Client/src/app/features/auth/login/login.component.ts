import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form!: FormGroup;
  loading = false;
  errorMessage = '';
  showPassword = false;
  private loginAttempts = 0;
  private readonly MAX_LOGIN_ATTEMPTS = 5;

  ngOnInit(): void {
    // If already logged in, redirect to dashboard
    if (this.auth.isLoggedIn()) {
      this.auth.redirectByRole();
      return;
    }

    // SECURITY: If 2FA session exists but user is on login page,
    // redirect to OTP page to complete verification
    if (this.auth.is2FASessionActive()) {
      console.log('Active 2FA session found, redirecting to OTP verification');
      this.router.navigate(['/auth/verify-otp']);
      return;
    }

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get email() { return this.form.get('email')!; }
  get password() { return this.form.get('password')!; }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // SECURITY: Rate limiting for login attempts
    this.loginAttempts++;
    if (this.loginAttempts > this.MAX_LOGIN_ATTEMPTS) {
      this.errorMessage = 'Too many failed attempts. Please try again later.';
      setTimeout(() => {
        this.loginAttempts = 0;
        this.errorMessage = '';
      }, 300000); // 5 minutes cooldown
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.auth.login(this.form.value).subscribe({
      next: (response) => {
        this.loading = false;
        this.loginAttempts = 0; // Reset on successful login attempt
        console.log('2FA initiated, redirecting to OTP verification');
        this.router.navigate(['/auth/verify-otp']);
      },
      error: (err) => {
        this.loading = false;
        console.error('Login error:', err);
        
        if (err.status === 401) {
          this.errorMessage = err.error?.error || 'Invalid email or password.';
        } else if (err.status === 400) {
          this.errorMessage = 'Please check your email and password.';
        } else if (err.status === 403) {
          this.errorMessage = 'Account locked. Please contact support.';
        } else if (err.status === 500) {
          this.errorMessage = 'Server error. Please try again later.';
        } else {
          this.errorMessage = 'Login failed. Please try again.';
        }
      }
    });
  }
}