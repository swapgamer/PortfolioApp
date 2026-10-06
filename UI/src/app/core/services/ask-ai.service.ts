import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AskAiResponse } from '../models/ask-ai.model';

@Injectable({ providedIn: 'root' })
export class AskAiService {
  private readonly http = inject(HttpClient);

  ask(query: string): Observable<AskAiResponse> {
    return this.http.post<AskAiResponse>(`${environment.apiBaseUrl}/api/ask-ai`, { query });
  }
}
