export interface ExperienceEntry {
  company: string;
  role: string;
  startDate: string; // ISO date (yyyy-MM-dd)
  endDate: string | null; // null = current position
  description: string;
  displayOrder: number;
}

export interface EducationEntry {
  institution: string;
  degree: string;
  startDate: string;
  endDate: string | null; // null = ongoing
  displayOrder: number;
}

export interface Testimonial {
  authorName: string;
  authorTitle: string | null;
  quote: string;
  displayOrder: number;
}
