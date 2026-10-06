import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TitleBar } from './title-bar';

describe('TitleBar', () => {
  let component: TitleBar;
  let fixture: ComponentFixture<TitleBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TitleBar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TitleBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
