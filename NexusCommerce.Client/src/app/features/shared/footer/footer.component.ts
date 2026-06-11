import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface SocialLink {
  name: string;
  label: string;
  href: string;
  icon: string;
}

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent {
  readonly year = new Date().getFullYear();

  readonly socialLinks: SocialLink[] = [
    { name: 'facebook', label: 'Facebook', href: 'https://facebook.com', icon: 'f' },
    { name: 'instagram', label: 'Instagram', href: 'https://instagram.com', icon: '◎' },
    { name: 'twitter', label: 'X (Twitter)', href: 'https://x.com', icon: '𝕏' },
    { name: 'linkedin', label: 'LinkedIn', href: 'https://linkedin.com', icon: 'in' }
  ];
}
