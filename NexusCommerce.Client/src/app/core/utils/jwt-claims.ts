import { jwtDecode } from 'jwt-decode';
import { DecodedToken } from '../models/models';

/** .NET ClaimTypes as serialized into JWT payloads by JwtSecurityTokenHandler */
const NAME_ID =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const GIVEN = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname';
const SURNAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname';
const ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function pickString(payload: Record<string, unknown>, ...keys: string[]): string | null {
  for (const k of keys) {
    const v = payload[k];
    if (typeof v === 'string' && v.length > 0) {
      return v;
    }
    if (Array.isArray(v) && v.length > 0 && typeof v[0] === 'string') {
      return v[0];
    }
  }
  return null;
}

/** Maps ASP.NET Identity JWT claims to the shape used by the app. */
export function decodeBuyonicToken(token: string): DecodedToken | null {
  try {
    const raw = jwtDecode<Record<string, unknown>>(token);
    const exp = raw['exp'];
    const expNum = typeof exp === 'number' ? exp : Number(exp);
    if (!Number.isFinite(expNum)) {
      return null;
    }

    const nameid =
      pickString(raw, NAME_ID, 'nameid', 'sub') ?? '';
    const email = pickString(raw, EMAIL, 'email') ?? '';
    const given_name = pickString(raw, GIVEN, 'given_name') ?? '';
    const family_name = pickString(raw, SURNAME, 'family_name') ?? '';
    const role = pickString(raw, ROLE, 'role') ?? '';

    return { nameid, email, given_name, family_name, role, exp: expNum };
  } catch {
    return null;
  }
}
