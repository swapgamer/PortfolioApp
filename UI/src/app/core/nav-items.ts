export interface NavItem {
  path: string;
  label: string;
  icon: string;
}

// Single source of truth for navigation -- TabBar and Sidebar both render from this list,
// so they can never drift out of sync with each other or with app.routes.ts.
export const NAV_ITEMS: NavItem[] = [
  { path: '', label: 'Home.cs', icon: '🏠' },
  { path: 'about', label: 'About.cs', icon: '👤' },
  { path: 'experience', label: 'Experience.cs', icon: '💼' },
  { path: 'education', label: 'Education.cs', icon: '🎓' },
  { path: 'projects', label: 'Projects.cs', icon: '📁' },
  { path: 'testimonials', label: 'Testimonials.cs', icon: '💬' },
  { path: 'contact', label: 'Contact.cs', icon: '✉️' },
  { path: 'blog', label: 'Blog.cs', icon: '📝' }
];
