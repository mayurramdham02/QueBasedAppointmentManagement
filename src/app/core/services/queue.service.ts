import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { 
  QueueItem, 
  Provider, 
  AddPatientDto, 
  QueueUpdateEvent 
} from '../models/queue.models';

/**
 * Queue Service
 * Handles all queue-related operations including HTTP calls and SignalR real-time updates
 */
@Injectable({
  providedIn: 'root'
})
export class QueueService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // Observable streams for real-time data
  private queueSubject = new BehaviorSubject<QueueItem[]>([]);
  private providersSubject = new BehaviorSubject<Provider[]>([]);
  private connectionStatusSubject = new BehaviorSubject<boolean>(false);

  public queue$ = this.queueSubject.asObservable();
  public providers$ = this.providersSubject.asObservable();
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeSignalR();
  }

  /**
   * Initialize SignalR connection
   */
  private initializeSignalR(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Handle queue updates from server
    this.hubConnection.on('QueueUpdated', (data: QueueUpdateEvent) => {
      console.log('Queue updated via SignalR:', data);
      this.queueSubject.next(data.queue);
      this.providersSubject.next(data.providers);
    });

    // Handle connection events
    this.hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.connectionStatusSubject.next(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.connectionStatusSubject.next(true);
      this.refreshQueue(); // Refresh data after reconnection
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR connection closed');
      this.connectionStatusSubject.next(false);
    });

    // Start connection
    this.startConnection();
  }

  /**
   * Start SignalR connection
   */
  private async startConnection(): Promise<void> {
    try {
      await this.hubConnection?.start();
      console.log('SignalR connected successfully');
      this.connectionStatusSubject.next(true);
      
      // Initial data load
      this.refreshQueue();
      this.refreshProviders();
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
      this.connectionStatusSubject.next(false);
      
      // Retry connection after 5 seconds
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  /**
   * Add a patient to the queue
   */
  addPatient(patient: AddPatientDto): Observable<QueueItem> {
    return this.http.post<QueueItem>(
      `${environment.apiUrl}/queue/add-patient`,
      patient
    );
  }

  /**
   * Get current queue
   */
  getQueue(): Observable<QueueItem[]> {
    return this.http.get<QueueItem[]>(
      `${environment.apiUrl}/queue/get-queue`
    );
  }

  /**
   * Get a specific queue item
   */
  getQueueItem(id: string): Observable<QueueItem> {
    return this.http.get<QueueItem>(
      `${environment.apiUrl}/queue/${id}`
    );
  }

  /**
   * Remove a patient from queue (accept by provider)
   */
  removePatient(queueItemId: string, providerId: string): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}/queue/${queueItemId}/remove`,
      { providerId }
    );
  }

  /**
   * Refresh queue data manually
   */
  refreshQueue(): void {
    this.getQueue().subscribe({
      next: (queue) => this.queueSubject.next(queue),
      error: (err) => console.error('Error refreshing queue:', err)
    });
  }

  /**
   * Refresh providers data manually
   */
  refreshProviders(): void {
    this.http.get<Provider[]>(`${environment.apiUrl}/provider`).subscribe({
      next: (providers) => this.providersSubject.next(providers),
      error: (err) => console.error('Error refreshing providers:', err)
    });
  }

  /**
   * Disconnect SignalR
   */
  disconnect(): void {
    this.hubConnection?.stop();
  }
}
