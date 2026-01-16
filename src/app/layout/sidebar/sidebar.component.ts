import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: string;
}

interface NavSection {
  title: string;
  icon: string;
  items: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  navSections: NavSection[] = [
    {
      title: 'Patient Portal',
      icon: '🏥',
      items: [
        {
          label: 'Patient Intake',
          icon: '📝',
          route: '/patient/intake'
        },
        {
          label: 'View Queue',
          icon: '👥',
          route: '/patient/queue'
        }
      ]
    },
    {
      title: 'Provider Dashboard',
      icon: '👨‍⚕️',
      items: [
        {
          label: 'Queue Management',
          icon: '📋',
          route: '/provider/dashboard'
        },
        {
          label: 'Provider Status',
          icon: '🟢',
          route: '/provider/status'
        }
      ]
    }
  ];

  isSidebarCollapsed = false;

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
