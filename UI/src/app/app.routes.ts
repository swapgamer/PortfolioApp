import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home)
  },
  {
    path: 'about',
    loadComponent: () => import('./pages/about/about').then((m) => m.About)
  },
  {
    path: 'experience',
    loadComponent: () => import('./pages/experience/experience').then((m) => m.Experience)
  },
  {
    path: 'education',
    loadComponent: () => import('./pages/education/education').then((m) => m.Education)
  },
  {
    path: 'projects',
    loadComponent: () => import('./pages/projects/projects').then((m) => m.Projects)
  },
  {
    path: 'testimonials',
    loadComponent: () => import('./pages/testimonials/testimonials').then((m) => m.Testimonials)
  },
  {
    path: 'contact',
    loadComponent: () => import('./pages/contact/contact').then((m) => m.Contact)
  },
  {
    path: 'blog',
    loadComponent: () => import('./pages/blog/blog').then((m) => m.Blog)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
