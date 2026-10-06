import { Component } from '@angular/core';
import testimonialsData from '../../core/content/testimonials.json';
import { Testimonial } from '../../core/models/content.model';

@Component({
  selector: 'app-testimonials',
  imports: [],
  templateUrl: './testimonials.html',
  styleUrl: './testimonials.css'
})
export class Testimonials {
  protected readonly entries: Testimonial[] = (testimonialsData as Testimonial[])
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder);
}
