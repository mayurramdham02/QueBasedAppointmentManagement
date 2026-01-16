import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { QueueService } from '../../../core/services/queue.service';
import { QueueItem, Provider } from '../../../core/models/queue.models';

@Component({
  selector: 'app-queue-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './queue-display.component.html',
  styleUrls: ['./queue-display.component.scss']
})
export class QueueDisplayComponent implements OnInit, OnDestroy {
  queue: QueueItem[] = [];
  providers: Provider[] = [];
  isConnected = false;
  isLoading = true;
  private destroy$ = new Subject<void>();

  constructor(private queueService: QueueService) {}

  ngOnInit(): void {
    this.subscribeToQueue();
    this.subscribeToProviders();
    this.subscribeToConnectionStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private subscribeToQueue(): void {
    this.queueService.queue$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (queue) => {
          this.queue = queue;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error receiving queue updates:', error);
          this.isLoading = false;
        }
      });
  }

  private subscribeToProviders(): void {
    this.queueService.providers$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (providers) => {
          this.providers = providers;
        },
        error: (error) => {
          console.error('Error receiving provider updates:', error);
        }
      });
  }

  private subscribeToConnectionStatus(): void {
    this.queueService.connectionStatus$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status) => {
          this.isConnected = status;
        }
      });
  }

  get onlineProvidersCount(): number {
    return this.providers.filter(p => p.isOnline).length;
  }

  get totalPatientsWaiting(): number {
    return this.queue.length;
  }

  getPainLevelClass(painLevel: number): string {
    if (painLevel >= 8) return 'severe';
    if (painLevel >= 5) return 'moderate';
    return 'mild';
  }

  getPainLevelLabel(painLevel: number): string {
    if (painLevel >= 8) return 'Severe';
    if (painLevel >= 5) return 'Moderate';
    return 'Mild';
  }

  formatWaitTime(minutes: number | null): string {
    if (minutes === null || minutes === undefined) return 'Calculating...';
    if (minutes === 0) return 'Next in line';
    if (minutes < 60) return `${minutes} min`;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  }

  formatJoinTime(joinTime: Date): string {
    const date = new Date(joinTime);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }

  refreshQueue(): void {
    this.queueService.refreshQueue();
    this.queueService.refreshProviders();
  }
}
