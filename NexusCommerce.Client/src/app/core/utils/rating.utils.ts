export type StarSlot = 'full' | 'half' | 'empty';

/** Rating shown only when the product has at least one customer review. */
export function resolveProductRating(rating?: number | null, reviewCount = 0): number | null {
  const count = Number(reviewCount) || 0;
  if (count <= 0) return null;

  const value = Number(rating);
  if (!Number.isFinite(value) || value <= 0) return null;

  return Math.min(5, Math.max(0, value));
}

export function getStarSlots(rating?: number | null, reviewCount = 0): StarSlot[] {
  const resolved = resolveProductRating(rating, reviewCount);
  if (resolved === null) {
    return Array.from({ length: 5 }, () => 'empty' as StarSlot);
  }

  const fullStars = Math.round(resolved);

  return Array.from({ length: 5 }, (_, index) =>
    index < fullStars ? 'full' : 'empty'
  );
}
