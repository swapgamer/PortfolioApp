import { Component, output } from '@angular/core';

@Component({
  selector: 'app-title-bar',
  imports: [],
  templateUrl: './title-bar.html',
  styleUrl: './title-bar.css'
})
export class TitleBar {
  toggleSidebar = output<void>();
}
