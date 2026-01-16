import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  private authService = inject(AuthService);
  
  @Input() isCollapsed = false;
  @Output() toggleSidebar = new EventEmitter<void>();

  user = this.authService.currentUser;

  currentUser: any = null;

  constructor() {
    this.user.subscribe(user => {
      this.currentUser = user;
    });
  }

  menuItems = [
    {
      title: 'Dashboard',
      icon: 'fas fa-home',
      route: '/dashboard',
      roles: ['user', 'admin']
    },
    {
      title: 'Analytics',
      icon: 'fas fa-chart-line',
      route: '/analytics',
      roles: ['admin']
    },
    {
      title: 'Users',
      icon: 'fas fa-users',
      route: '/users',
      roles: ['admin']
    },
    {
      title: 'Settings',
      icon: 'fas fa-cog',
      route: '/settings',
      roles: ['user', 'admin']
    }
  ];

  get filteredMenuItems() {
    const userRole = this.currentUser?.role || 'user';
    return this.menuItems.filter(item => item.roles.includes(userRole));
  }

  onToggleSidebar() {
    this.toggleSidebar.emit();
  }

  logout() {
    this.authService.logout();
  }
}
