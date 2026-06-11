/** Safe message for HttpClient error callbacks (avoids "[object Object]" from ProblemDetails, etc.). */
export function apiErrorMessage(err: unknown, fallback: string): string {
  const body = (err as { error?: unknown })?.error;
  if (typeof body === 'string') return body;
  if (body && typeof body === 'object') {
    const o = body as Record<string, unknown>;
    if (typeof o['title'] === 'string') return o['title'];
    if (typeof o['message'] === 'string') return o['message'];
    if (typeof o['detail'] === 'string') return o['detail'];
  }
  return fallback;
}
