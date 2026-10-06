import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Highlight {
  icon: string;
  text: string;
}

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
  protected readonly highlights: Highlight[] = [
    { icon: '🏗️', text: 'Built and maintained ASP.NET Core Web APIs backed by SQL Server' },
    { icon: '🅰️', text: 'Developed responsive front ends with Angular and TypeScript' },
    { icon: '🗄️', text: 'Designed relational schemas and tuned queries in SSMS' },
    { icon: '🔧', text: '3 years of hands-on experience across the full .NET stack' },
    { icon: '🤝', text: 'Comfortable owning a feature end-to-end, from schema to UI' }
  ];
}
