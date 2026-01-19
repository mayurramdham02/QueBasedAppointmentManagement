import { Component, OnInit, OnDestroy, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { QueueService } from '../../../core/services/queue.service';
import { ProviderService } from '../../../core/services/provider.service';
import { Provider } from '../../../core/models/queue.models';

@Component({
  selector: 'app-provider-status',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './provider-status.component.html',
  styleUrls: ['./provider-status.component.scss']
})
export class ProviderStatusComponent implements OnInit, OnDestroy {
  providers: Provider[] = [];
  selectedProviderId: string = '';
  isLoading = true;
  isProcessing = false;
  private destroy$ = new Subject<void>();


  constructor(
    private queueService: QueueService,
    private providerService: ProviderService,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.subscribeToProviders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private subscribeToProviders(): void {
    this.queueService.providers$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (providers) => {
          this.ngZone.run(() => {
            this.providers = providers;
            this.isLoading = false;
            
            // Restore selected provider if exists and valid
            const storedId = localStorage.getItem('selectedProviderId');
            if (storedId && providers.some(p => p.id === storedId)) {
              this.selectedProviderId = storedId;
            } else if (!this.selectedProviderId && providers.length > 0) {
              // Default to first if nothing selected
               this.selectedProviderId = providers[0].id;
            }
          });
        },
        error: (error) => {
          console.error('Error receiving provider updates:', error);
          this.ngZone.run(() => {
            this.isLoading = false;
          });
        }
      });
  }

  get selectedProvider(): Provider | undefined {
    return this.providers.find(p => p.id === this.selectedProviderId);
  }

  onProviderChange(): void {
    if (this.selectedProviderId) {
      localStorage.setItem('selectedProviderId', this.selectedProviderId);
    }
  }

  toggleStatus(): void {
    const provider = this.selectedProvider;
    if (!provider || this.isProcessing) return;

    this.isProcessing = true;
    const newStatus = !provider.isOnline; // Logic fixed

    this.providerService.toggleOnlineStatus(provider.id, newStatus).subscribe({
      next: () => {
        // Optimistic update handled by SignalR subscription
        this.ngZone.run(() => {
          this.isProcessing = false;
        });
      },
      error: (error) => {
        console.error('Error toggling status:', error);
        alert('Failed to update status. Please try again.');
        this.ngZone.run(() => {
          this.isProcessing = false;
        });
      }
    });
  }

  refreshProviders(): void {
    this.isLoading = true;
    this.queueService.refreshProviders();
  }
}
