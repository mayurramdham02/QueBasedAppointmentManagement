import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/patient/intake',
    pathMatch: 'full'
  },
  {
    path: 'patient',
    children: [
      {
        path: 'intake',
        loadComponent: () => import('./features/patient/patient-intake/patient-intake.component')
          .then(c => c.PatientIntakeComponent)
      },
      {
        path: 'queue',
        loadComponent: () => import('./features/patient/queue-display/queue-display.component')
          .then(c => c.QueueDisplayComponent)
      }
    ]
  },
  {
    path: 'provider',
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/provider/provider-dashboard/provider-dashboard.component')
          .then(c => c.ProviderDashboardComponent)
      },
      {
        path: 'status',
        loadComponent: () => import('./features/provider/provider-status/provider-status.component')
          .then(c => c.ProviderStatusComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/patient/intake'
  }
];
