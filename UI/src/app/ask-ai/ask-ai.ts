import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, HostListener, effect, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AskAiService } from '../core/services/ask-ai.service';

@Component({
  selector: 'app-ask-ai',
  imports: [FormsModule],
  templateUrl: './ask-ai.html',
  styleUrl: './ask-ai.css'
})
export class AskAi {
  private readonly askAiService = inject(AskAiService);
  private readonly queryInput = viewChild<ElementRef<HTMLInputElement>>('queryInput');

  protected readonly open = signal(false);
  protected readonly query = signal('');
  protected readonly loading = signal(false);
  protected readonly answer = signal<string | null>(null);
  protected readonly sources = signal<string[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    // Autofocus the query input whenever the dialog opens.
    effect(() => {
      if (this.open()) {
        queueMicrotask(() => this.queryInput()?.nativeElement.focus());
      }
    });
  }

  @HostListener('document:keydown', ['$event'])
  protected onKeydown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.toggle();
    } else if (event.key === 'Escape' && this.open()) {
      this.close();
    }
  }

  protected toggle(): void {
    this.open.update((isOpen) => !isOpen);
  }

  protected close(): void {
    this.open.set(false);
  }

  protected onQueryInput(value: string): void {
    this.query.set(value);
  }

  protected submit(): void {
    const q = this.query().trim();
    if (!q || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);
    this.answer.set(null);
    this.sources.set([]);

    this.askAiService.ask(q).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.answer.set(res.answer);
        this.sources.set(res.sources);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(
          err.status === 429
            ? "You're asking too quickly. Please wait a moment and try again."
            : "Couldn't reach the AI assistant right now. Please try again in a moment."
        );
      }
    });
  }
}
