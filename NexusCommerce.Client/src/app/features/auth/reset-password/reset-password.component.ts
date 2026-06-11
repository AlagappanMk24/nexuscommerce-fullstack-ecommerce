// import { Component, inject, OnInit } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
// import { RouterLink, ActivatedRoute, Router } from '@angular/router';
// import { AuthService } from '../../../core/services/auth.service';

// function passwordMatch(control: AbstractControl) {
//   const pw = control.get('newPassword')?.value;
//   const cpw = control.get('confirmPassword')?.value;
//   return pw === cpw ? null : { mismatch: true };
// }

// @Component({
//   selector: 'app-reset-password',
//   standalone: true,
//   imports: [CommonModule, ReactiveFormsModule, RouterLink],
//   templateUrl: './reset-password.component.html',
//   styleUrls: ['./reset-password.component.scss']
// })
// export class ResetPasswordComponent implements OnInit {
//   private fb = inject(FormBuilder);
//   private auth = inject(AuthService);
//   private route = inject(ActivatedRoute);
//   private router = inject(Router);

//   form!: FormGroup;
//   loading = false;
//   errorMessage = '';
//   successMessage = '';
//   showPassword = false;

//   ngOnInit(): void {
//     const email = this.route.snapshot.queryParamMap.get('email') || '';
//     const token = this.route.snapshot.queryParamMap.get('token') || '';

//     this.form = this.fb.group({
//       email: [email, [Validators.required, Validators.email]],
//       token: [token, Validators.required],
//       newPassword: ['', [Validators.required, Validators.minLength(6)]],
//       confirmPassword: ['', Validators.required]
//     }, { validators: passwordMatch });
//   }

//   get f() { return this.form.controls; }

//   onSubmit(): void {
//     if (this.form.invalid) {
//       this.form.markAllAsTouched();
//       return;
//     }

//     this.loading = true;
//     this.errorMessage = '';

//     this.auth.resetPassword(this.form.value).subscribe({
//       next: () => {
//         this.loading = false;
//         this.successMessage = 'Password reset successful! Redirecting to login…';
//         setTimeout(() => this.router.navigate(['/auth/login']), 2500);
//       },
//       error: (err) => {
//         this.loading = false;
//         if (err.status === 400) {
//           this.errorMessage = err.error || 'Invalid or expired token.';
//         } else {
//           this.errorMessage = 'Server error. Please try again.';
//         }
//       }
//     });
//   }
// }
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatchValidator(control: AbstractControl) {
  const password = control.get('newPassword')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password === confirm ? null : { mismatch: true };
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  form!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token') || '';
    const email = this.route.snapshot.queryParamMap.get('email') || '';

    if (!token || !email) {
      this.router.navigate(['/auth/forgot-password']);
      return;
    }

    this.form = this.fb.group({
      email: [email, [Validators.required, Validators.email]],
      token: [token, Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordMatchValidator });
  }

  get f() { return this.form.controls; }

  get hasMinLength(): boolean {
    return this.f['newPassword']?.value?.length >= 6;
  }

  get hasLowercase(): boolean {
    return /[a-z]/.test(this.f['newPassword']?.value);
  }

  get hasUppercase(): boolean {
    return /[A-Z]/.test(this.f['newPassword']?.value);
  }

  get hasNumber(): boolean {
    return /[0-9]/.test(this.f['newPassword']?.value);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.auth.resetPassword(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Password reset successful! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2500);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Invalid or expired token. Please request a new reset link.';
      }
    });
  }
}