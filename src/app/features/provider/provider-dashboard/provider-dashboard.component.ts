import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { QueueService } from '../../../core/services/queue.service';
import { ProviderService } from '../../../core/services/provider.service';
import { QueueItem, Provider } from '../../../core/models/queue.models';

@Component({
  selector: 'app-provider-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './provider-dashboard.component.html',
  styleUrls: ['./provider-dashboard.component.scss']
})
export class ProviderDashboardComponent implements OnInit, OnDestroy {
  queue: QueueItem[] = [];
  providers: Provider[] = [];
  selectedProvider: Provider | null = null;
  isConnected = false;
  isLoading = true;
  isProcessing = false;
  private destroy$ = new Subject<void>();

  constructor(
    private queueService: QueueService,
    private providerService: ProviderService
  ) {}

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
          // Auto-select first provider if none selected
          if (!this.selectedProvider && providers.length > 0) {
            this.selectedProvider = providers[0];
          }
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

  selectProvider(provider: Provider): void {
    this.selectedProvider = provider;
  }

  toggleProviderStatus(provider: Provider): void {
    if (this.isProcessing) return;

    this.isProcessing = true;
    const newStatus = !provider.isOnline;

    this.providerService.toggleOnlineStatus(provider.id, newStatus).subscribe({
      next: () => {
        console.log(`Provider ${provider.providerName} is now ${newStatus ? 'online' : 'offline'}`);
        this.isProcessing = false;
      },
      error: (error) => {
        console.error('Error toggling provider status:', error);
        alert('Failed to update provider status. Please try again.');
        this.isProcessing = false;
      }
    });
  }

  acceptPatient(patient: QueueItem): void {
    if (!this.selectedProvider) {
      alert('Please select a provider first');
      return;
    }

    if (!this.selectedProvider.isOnline) {
      alert('Provider must be online to accept patients');
      return;
    }

    if (this.isProcessing) return;

    const confirmMessage = `Accept patient "${patient.patientName}" for ${this.selectedProvider.providerName}?`;
    if (!confirm(confirmMessage)) return;

    this.isProcessing = true;

    this.queueService.removePatient(patient.id, this.selectedProvider.id).subscribe({
      next: () => {
        console.log(`Patient ${patient.patientName} accepted by ${this.selectedProvider?.providerName}`);
        this.isProcessing = false;
      },
      error: (error) => {
        console.error('Error accepting patient:', error);
        alert(error.error?.error || 'Failed to accept patient. Please try again.');
        this.isProcessing = false;
      }
    });
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
    if (minutes === 0) return 'Next';
    if (minutes < 60) return `${minutes}m`;
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

  get onlineProvidersCount(): number {
    return this.providers.filter(p => p.isOnline).length;
  }

  get totalPatientsWaiting(): number {
    return this.queue.length;
  }
}
