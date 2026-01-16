import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterModule],
  template: `
    <mat-card class="error-card">
      <mat-card-header>
        <mat-card-title>Page Not Found</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <p>The page you're looking for doesn't exist.</p>
      </mat-card-content>
      <mat-card-actions>
        <button mat-button routerLink="/">Go to Home</button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .error-card {
      max-width: 400px;
      margin: 100px auto;
      text-align: center;
    }
  `]
})
export class NotFoundComponent {}
