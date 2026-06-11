import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.scss']
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form!: FormGroup;
  otpControls: FormControl[] = [];
  loading = false;
  resending = false;
  errorMessage = '';
  successMessage = '';
  timeRemaining = 120; // 2 minutes
  private timerInterval: any;

  ngOnInit(): void {
    // SECURITY: Check if 2FA session exists - if not, redirect to login
    if (!this.auth.is2FASessionActive()) {
      console.warn('Security: No 2FA session found, redirecting to login');
      this.router.navigate(['/auth/login']);
      return;
    }

    // SECURITY: Check if user is already logged in
    if (this.auth.isLoggedIn()) {
      console.warn('Security: User already logged in, redirecting to dashboard');
      this.auth.redirectByRole();
      return;
    }

    // Create OTP form controls (6 digits)
    for (let i = 0; i < 6; i++) {
      this.otpControls.push(new FormControl('', [Validators.required, Validators.pattern('[0-9]')]));
    }

    this.form = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern('^[0-9]{6}$')]]
    });

    // Subscribe to OTP control changes to combine into single value
    this.otpControls.forEach((control, index) => {
      control.valueChanges.subscribe(() => {
        const otpValue = this.otpControls.map(c => c.value).join('');
        this.form.patchValue({ otp: otpValue }, { emitEvent: false });
      });
    });

    this.startTimer();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  getOtpControl(index: number): FormControl {
    return this.otpControls[index];
  }

  get otpInvalid(): boolean {
    return this.form.get('otp')?.invalid || false;
  }

  isOtpComplete(): boolean {
    return this.otpControls.every(control => control.value && control.value.length === 1);
  }

  onOtpInput(event: any, index: number): void {
    const input = event.target;
    const value = input.value;
    
    // Only allow digits
    if (value && !/^[0-9]$/.test(value)) {
      input.value = '';
      return;
    }
    
    // Auto-focus next input
    if (value && index < 5) {
      const nextInput = this.getInputElement(index + 1);
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  onOtpKeydown(event: KeyboardEvent, index: number): void {
    const input = event.target as HTMLInputElement;
    
    // Handle backspace - move to previous input
    if (event.key === 'Backspace' && !input.value && index > 0) {
      const prevInput = this.getInputElement(index - 1);
      if (prevInput) {
        prevInput.focus();
        prevInput.value = '';
        this.otpControls[index - 1].setValue('');
      }
    }
    
    // Handle paste
    if (event.key === 'v' && (event.ctrlKey || event.metaKey)) {
      event.preventDefault();
      this.handlePaste();
    }
  }

  private getInputElement(index: number): HTMLInputElement | null {
    const inputs = document.querySelectorAll('.otp-input');
    return inputs[index] as HTMLInputElement || null;
  }

  private handlePaste(): void {
    navigator.clipboard.readText().then(text => {
      const digits = text.trim().replace(/\D/g, '').split('');
      if (digits.length === 6) {
        digits.forEach((digit, idx) => {
          if (idx < 6 && /^[0-9]$/.test(digit)) {
            this.otpControls[idx].setValue(digit);
            const input = this.getInputElement(idx);
            if (input) input.value = digit;
          }
        });
        // Focus on last input
        const lastInput = this.getInputElement(5);
        if (lastInput) lastInput.focus();
      }
    });
  }

  startTimer(): void {
    this.timerInterval = setInterval(() => {
      if (this.timeRemaining > 0) {
        this.timeRemaining--;
      } else {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  resendOtp(): void {
    this.resending = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    this.auth.resendOtp().subscribe({
      next: (response) => {
        this.resending = false;
        this.successMessage = response.message || 'A new verification code has been sent to your email.';
        this.timeRemaining = 120;
        this.startTimer();
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (err) => {
        this.resending = false;
        console.error('Resend OTP error:', err);
        
        if (err.status === 404) {
          this.errorMessage = err.error?.error || 'Session expired. Please login again.';
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 2000);
        } else {
          this.errorMessage = err.error?.error || 'Failed to resend code. Please try again.';
        }
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || !this.isOtpComplete()) {
      this.otpControls.forEach(control => control.markAsTouched());
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const otpCode = this.otpControls.map(c => c.value).join('');
    
    this.auth.validateOtp(otpCode).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = 'Verification successful! Redirecting...';
        
        setTimeout(() => {
          this.auth.redirectByRole();
        }, 1500);
      },
      error: (err) => {
        this.loading = false;
        console.error('OTP validation error:', err);
        
        if (err.status === 401) {
          this.errorMessage = err.error?.error || 'Invalid or expired verification code. Please try again.';
          
          // Clear OTP fields on error
          this.otpControls.forEach(control => {
            control.setValue('');
            control.markAsUntouched();
          });
          
          // Focus on first input
          const firstInput = this.getInputElement(0);
          if (firstInput) firstInput.focus();
        } else {
          this.errorMessage = err.error?.error || 'Verification failed. Please try again.';
        }
      }
    });
  }

  // Helper method for template to access router
  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}