import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterModule],
  template: `
    <mat-card class="error-card">
      <mat-card-header>
        <mat-card-title>Access Denied</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <p>You don't have permission to access this page.</p>
      </mat-card-content>
      <mat-card-actions>
        <button mat-button routerLink="/dashboard">Go to Dashboard</button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .error-card {
      max-width: 400px;
      margin: 2rem auto;
      text-align: center;
    }
  `]
})
export class UnauthorizedComponent {}
