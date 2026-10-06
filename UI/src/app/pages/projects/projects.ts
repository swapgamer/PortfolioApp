import { Component, inject, signal } from '@angular/core';
import { GithubService } from '../../core/services/github.service';
import { GithubRepo } from '../../core/models/github-repo.model';

@Component({
  selector: 'app-projects',
  imports: [],
  templateUrl: './projects.html',
  styleUrl: './projects.css'
})
export class Projects {
  private readonly githubService = inject(GithubService);

  protected readonly repos = signal<GithubRepo[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);

  constructor() {
    this.githubService.getRepos().subscribe({
      next: (repos) => {
        this.repos.set(repos);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }
}
