/**
 * Queue Status Enum
 * Matches backend QueueStatus enum
 */
export enum QueueStatus {
  Waiting = 'Waiting',
  Accepted = 'Accepted',
  Removed = 'Removed',
  InProgress = 'InProgress',
  Completed = 'Completed'
}

/**
 * Queue Item Model
 * Represents a patient in the queue
 */
export interface QueueItem {
  id: string;
  patientName: string;
  symptom: string;
  painLevel: number;
  joinTime: Date;
  estimatedWaitTimeMinutes: number | null;
  status: QueueStatus;
  acceptedByProviderId: string | null;
  acceptedByProvider: Provider | null;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * Provider Model
 * Represents a healthcare provider
 */
export interface Provider {
  id: string;
  providerName: string;
  isOnline: boolean;
  lastActivityTime: Date | null;
  acceptedPatients: QueueItem[];
  createdAt: Date;
  updatedAt: Date;
}

/**
 * DTO for adding a patient to the queue
 */
export interface AddPatientDto {
  patientName: string;
  symptom: string;
  painLevel: number;
}

/**
 * DTO for toggling provider online status
 */
export interface ToggleProviderStatusDto {
  isOnline: boolean;
}

/**
 * DTO for removing a patient from queue
 */
export interface RemovePatientDto {
  providerId: string;
}

/**
 * SignalR Queue Update Event
 */
export interface QueueUpdateEvent {
  queue: QueueItem[];
  providers: Provider[];
  timestamp: Date;
}
