import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { AskAi } from './ask-ai';

describe('AskAi', () => {
  let component: AskAi;
  let fixture: ComponentFixture<AskAi>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AskAi],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AskAi);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
