import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { getStarSlots, resolveProductRating, StarSlot } from '../../../core/utils/rating.utils';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './star-rating.component.html',
  styleUrls: ['./star-rating.component.scss']
})
export class StarRatingComponent implements OnChanges {
  @Input() rating?: number | null;
  @Input() reviewCount = 0;
  @Input() showNumeric = true;
  @Input() showCount = true;
  @Input() size: 'sm' | 'md' = 'sm';

  slots: StarSlot[] = [];
  effectiveRating: number | null = null;
  normalizedReviewCount = 0;
  ariaLabel = 'No reviews yet';

  ngOnChanges(): void {
    this.normalizedReviewCount = Number(this.reviewCount) || 0;
    this.effectiveRating = resolveProductRating(this.rating, this.normalizedReviewCount);
    this.slots = getStarSlots(this.rating, this.normalizedReviewCount);

    if (this.effectiveRating === null) {
      this.ariaLabel = 'No reviews yet';
    } else {
      this.ariaLabel = `Rated ${this.effectiveRating.toFixed(1)} out of 5 from ${this.normalizedReviewCount} reviews`;
    }
  }
}
