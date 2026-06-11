import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatch(control: AbstractControl) {
  const pw = control.get('password')?.value;
  const cpw = control.get('confirmPassword')?.value;
  return pw === cpw ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;
  step = 1; // multi-step form

  ngOnInit(): void {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName:  ['', [Validators.required, Validators.minLength(2)]],
      email:     ['', [Validators.required, Validators.email]],
      password:  ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      accountType: ['Customer', Validators.required],
      storeName: [''],
      address:   ['']
    }, { validators: passwordMatch });

    // Toggle storeName required validator based on accountType
    this.form.get('accountType')!.valueChanges.subscribe(type => {
      const storeCtrl = this.form.get('storeName')!;
      if (type === 'Seller') {
        storeCtrl.setValidators([Validators.required, Validators.maxLength(100)]);
      } else {
        storeCtrl.clearValidators();
      }
      storeCtrl.updateValueAndValidity();
    });
  }

  get f() { return this.form.controls; }
  get isSeller() { return this.form.get('accountType')?.value === 'Seller'; }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.auth.register(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Registration successful! Redirecting to login…';
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
error: (err) => {

  this.loading = false;

  try {

    const responseError =
      typeof err.error === 'string'
        ? JSON.parse(err.error)
        : err.error;

    // لو Array جاية من Identity
    if (Array.isArray(responseError)) {

      this.errorMessage = responseError
        .map((e: any) => e.description)
        .join(' ');

    }

    // Validation Errors
    else if (responseError?.errors) {

      this.errorMessage = Object.values(responseError.errors)
        .flat()
        .join(' ');

    }

    // String عادي
    else if (typeof responseError === 'string') {

      this.errorMessage = responseError;

    }

    else {

      this.errorMessage = 'Registration failed.';
    }

  } catch {

    this.errorMessage = 'Server error. Please try again later.';
  }
}
    });
  }
}
