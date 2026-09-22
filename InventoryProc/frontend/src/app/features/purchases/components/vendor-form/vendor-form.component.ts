import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { VendorService } from '../../services/vendor.service';
import { Vendor, CreateVendorRequest, UpdateVendorRequest } from '../../models/vendor.model';

@Component({
  selector: 'app-vendor-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './vendor-form.component.html',
  styleUrls: ['./vendor-form.component.css']
})
export class VendorFormComponent implements OnInit {
  vendorForm: FormGroup;
  isEditMode: boolean = false;
  vendorId: string | null = null;
  loading: boolean = false;
  error: string | null = null;
  submitted: boolean = false;

  constructor(
    private fb: FormBuilder,
    private vendorService: VendorService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.vendorForm = this.fb.group({
      code: ['', Validators.required],
      companyName: ['', Validators.required],
      contactPerson: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      mobile: ['', Validators.required],
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city: ['', Validators.required],
      state: ['', Validators.required],
      postalCode: ['', Validators.required],
      country: ['', Validators.required],
      gstNumber: [''],
      panNumber: [''],
      creditLimit: [0, [Validators.required, Validators.min(0)]],
      paymentTermDays: [0, [Validators.required, Validators.min(0)]],
      bankName: [''],
      bankAccountNumber: [''],
      bankIFSCCode: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.vendorId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.vendorId && this.vendorId !== 'new';

    if (this.isEditMode) {
      this.loadVendor();
      this.vendorForm.get('code')?.disable();
    }
  }

  loadVendor(): void {
    if (!this.vendorId) return;

    this.loading = true;
    this.vendorService.getById(this.vendorId).subscribe({
      next: (response) => {
        if (response.success) {
          this.vendorForm.patchValue(response.data);
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load vendor. Please try again.';
        this.loading = false;
        console.error('Error loading vendor:', err);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.vendorForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = null;

    if (this.isEditMode) {
      this.updateVendor();
    } else {
      this.createVendor();
    }
  }

  createVendor(): void {
    const request: CreateVendorRequest = this.vendorForm.value;

    this.vendorService.create(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/purchases/vendors']);
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'Failed to create vendor. Please try again.';
        this.loading = false;
        console.error('Error creating vendor:', err);
      }
    });
  }

  updateVendor(): void {
    if (!this.vendorId) return;

    const request: UpdateVendorRequest = {
      ...this.vendorForm.value,
      code: undefined
    };

    this.vendorService.update(this.vendorId, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/purchases/vendors']);
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'Failed to update vendor. Please try again.';
        this.loading = false;
        console.error('Error updating vendor:', err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases/vendors']);
  }

  get f() {
    return this.vendorForm.controls;
  }
}
