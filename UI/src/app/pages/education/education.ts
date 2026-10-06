import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import educationData from '../../core/content/education.json';
import { EducationEntry } from '../../core/models/content.model';

@Component({
  selector: 'app-education',
  imports: [DatePipe],
  templateUrl: './education.html',
  styleUrl: './education.css'
})
export class Education {
  protected readonly entries: EducationEntry[] = (educationData as EducationEntry[])
    .slice()
    .sort((a, b) => a.displayOrder - b.displayOrder);
}
