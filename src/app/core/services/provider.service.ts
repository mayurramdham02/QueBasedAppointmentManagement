import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Provider, ToggleProviderStatusDto } from '../models/queue.models';

/**
 * Provider Service
 * Handles provider-related operations
 */
@Injectable({
  providedIn: 'root'
})
export class ProviderService {
  constructor(private http: HttpClient) {}

  /**
   * Get all providers
   */
  getAllProviders(): Observable<Provider[]> {
    return this.http.get<Provider[]>(`${environment.apiUrl}/provider`);
  }

  /**
   * Get a specific provider
   */
  getProvider(id: string): Observable<Provider> {
    return this.http.get<Provider>(`${environment.apiUrl}/provider/${id}`);
  }

  /**
   * Toggle provider online/offline status
   */
  toggleOnlineStatus(id: string, isOnline: boolean): Observable<any> {
    const dto: ToggleProviderStatusDto = { isOnline };
    return this.http.put(
      `${environment.apiUrl}/provider/${id}/toggle-online`,
      dto
    );
  }
}
