import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import experienceData from '../../core/content/experience.json';
import { ExperienceEntry } from '../../core/models/content.model';

@Component({
  selector: 'app-experience',
  imports: [DatePipe],
  templateUrl: './experience.html',
  styleUrl: './experience.css'
})
export class Experience {
  protected readonly entries: ExperienceEntry[] = (experienceData as ExperienceEntry[])
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder);
}
