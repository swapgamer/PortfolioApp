import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ContactRequest } from '../models/contact-request.model';

@Injectable({ providedIn: 'root' })
export class ContactService {
  private readonly http = inject(HttpClient);

  submit(request: ContactRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/contact`, request);
  }
}
