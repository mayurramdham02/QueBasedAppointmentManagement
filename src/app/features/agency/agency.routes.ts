import { Routes } from '@angular/router';

export const agencyRoutes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      },
      {
        path: 'login',
        //component: //add your component here
      }
    ]
  }
];
