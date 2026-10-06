import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TitleBar } from './title-bar/title-bar';
import { TabBar } from './tab-bar/tab-bar';
import { Sidebar } from './sidebar/sidebar';
import { AskAi } from '../ask-ai/ask-ai';
import { NAV_ITEMS } from '../core/nav-items';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, TitleBar, TabBar, Sidebar, AskAi],
  templateUrl: './shell.html',
  styleUrl: './shell.css'
})
export class Shell {
  protected readonly navItems = NAV_ITEMS;
  protected readonly sidebarOpen = signal(false);

  protected toggleSidebar(): void {
    this.sidebarOpen.update((open) => !open);
  }
}
