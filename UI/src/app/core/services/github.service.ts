import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GithubRepo } from '../models/github-repo.model';

// Public, unauthenticated GitHub REST API call -- no key needed, subject to GitHub's
// unauthenticated rate limit, which is acceptable for a low-traffic personal site
// (Docs/02-High-Level-Design.md section 2.1).
const GITHUB_USERNAME = 'swapgamer';

@Injectable({ providedIn: 'root' })
export class GithubService {
  private readonly http = inject(HttpClient);

  getRepos(): Observable<GithubRepo[]> {
    return this.http.get<GithubRepo[]>(`https://api.github.com/users/${GITHUB_USERNAME}/repos`, {
      params: { sort: 'updated', per_page: '10' }
    });
  }
}
