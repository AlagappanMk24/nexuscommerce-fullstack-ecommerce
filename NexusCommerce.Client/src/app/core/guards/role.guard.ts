import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigate(['/auth/login']);
    return false;
  }

  const requiredRole: string | string[] = route.data['role'];
  const userRole = auth.getRole();

  if (!userRole) {
    router.navigate(['/unauthorized']);
    return false;
  }

  const allowed = Array.isArray(requiredRole)
    ? requiredRole.includes(userRole)
    : userRole === requiredRole;

  if (!allowed) {
    router.navigate(['/unauthorized']);
    return false;
  }

  return true;
};
