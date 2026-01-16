import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { QueueService } from '../../../core/services/queue.service';
import { AddPatientDto } from '../../../core/models/queue.models';

@Component({
  selector: 'app-patient-intake',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-intake.component.html',
  styleUrls: ['./patient-intake.component.scss']
})
export class PatientIntakeComponent implements OnInit {
  intakeForm!: FormGroup;
  isSubmitting = false;
  submitSuccess = false;
  submitError: string | null = null;

  // Pain level labels for slider
  painLevels = [
    { value: 1, label: 'Minimal', color: '#43A047' },
    { value: 2, label: 'Mild', color: '#66BB6A' },
    { value: 3, label: 'Mild', color: '#8BC34A' },
    { value: 4, label: 'Moderate', color: '#CDDC39' },
    { value: 5, label: 'Moderate', color: '#FFEB3B' },
    { value: 6, label: 'Moderate', color: '#FFC107' },
    { value: 7, label: 'Severe', color: '#FF9800' },
    { value: 8, label: 'Severe', color: '#FF6F00' },
    { value: 9, label: 'Very Severe', color: '#F44336' },
    { value: 10, label: 'Worst Pain', color: '#D32F2F' }
  ];

  constructor(
    private fb: FormBuilder,
    private queueService: QueueService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.intakeForm = this.fb.group({
      patientName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      symptom: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]],
      painLevel: [5, [Validators.required, Validators.min(1), Validators.max(10)]]
    });
  }

  get f() {
    return this.intakeForm.controls;
  }

  get selectedPainLevel() {
    const level = this.intakeForm.get('painLevel')?.value || 5;
    return this.painLevels[level - 1];
  }

  onSubmit(): void {
    if (this.intakeForm.invalid) {
      this.intakeForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.submitError = null;

    const patientData: AddPatientDto = {
      patientName: this.intakeForm.value.patientName.trim(),
      symptom: this.intakeForm.value.symptom.trim(),
      painLevel: this.intakeForm.value.painLevel
    };

    this.queueService.addPatient(patientData).subscribe({
      next: (response) => {
        console.log('Patient added successfully:', response);
        this.submitSuccess = true;
        this.isSubmitting = false;
        
        // Show success message for 2 seconds, then redirect to queue view
        setTimeout(() => {
          this.router.navigate(['/patient/queue']);
        }, 2000);
      },
      error: (error) => {
        console.error('Error adding patient:', error);
        this.submitError = error.error?.error || 'Failed to add patient to queue. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  resetForm(): void {
    this.intakeForm.reset({
      patientName: '',
      symptom: '',
      painLevel: 5
    });
    this.submitSuccess = false;
    this.submitError = null;
  }
}
