import { Component } from '@angular/core';

@Component({
  selector: 'app-about',
  imports: [],
  templateUrl: './about.html',
  styleUrl: './about.css'
})
export class About {
  protected readonly skills: string[] = [
    'C#',
    'ASP.NET Core Web API',
    'Entity Framework Core',
    'SQL Server (SSMS)',
    'Angular',
    'TypeScript',
    'REST APIs',
    'Git'
  ];
}
