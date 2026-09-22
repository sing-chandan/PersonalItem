# HTML Template Updates - Products-Style Design

## Overview

To achieve consistent look and feel across all pages, we need to update the HTML structure from **table-based layouts** to **card-grid layouts** matching the Products pages.

## ✅ Completed Updates

### Purchases Module
1. **vendor-list.component.html** - ✅ Updated to card-grid layout
2. **vendor-form.component.html** - ✅ Updated to form-section layout

## 🔄 Remaining Updates Needed

### Purchases Module (4 files)
3. **purchase-order-list.component.html** - Needs card-grid layout
4. **purchase-order-form.component.html** - Needs form-section layout
5. **grn-list.component.html** - Needs card-grid layout
6. **grn-form.component.html** - Needs form-section layout

### Reports Module (3 files)
7. **sales-report.component.html** - Keep table but add grid for summary cards
8. **purchase-report.component.html** - Keep table but add grid for summary cards
9. **inventory-report.component.html** - Keep table but add grid for summary cards

### Users Module (5 files)
10. **user-list.component.html** - Needs card-grid layout
11. **user-form.component.html** - Needs form-section layout
12. **user-profile.component.html** - Needs info-row grid layout
13. **user-activity.component.html** - Keep table but modernize
14. **activity-logs.component.html** - Keep table but modernize

---

## Design Patterns

### Pattern 1: List/Grid View (for vendor-list, PO-list, user-list, etc.)

```html
<div class="[module]-list-container">
  <div class="header">
    <h1>Title</h1>
    <div class="header-actions">
      <button class="btn btn-primary">+ Add New</button>
    </div>
  </div>

  <div class="filters">
    <div class="search-box">
      <input type="text" class="search-input" placeholder="Search..." />
      <button class="btn btn-secondary">Search</button>
    </div>
  </div>

  <div class="error-message" *ngIf="error">{{ error }}</div>
  <div class="loading" *ngIf="loading">Loading...</div>
  <div class="no-data" *ngIf="!loading && items.length === 0">No items found.</div>

  <div class="items-grid" *ngIf="!loading && items.length > 0">
    <div class="item-card" *ngFor="let item of items">
      <div class="item-header">
        <h3>{{ item.name }}</h3>
        <span class="item-code">{{ item.code }}</span>
      </div>

      <div class="item-info">
        <div class="info-row">
          <span class="label">Label:</span>
          <span class="value">{{ item.value }}</span>
        </div>
      </div>

      <div class="item-actions">
        <button class="btn btn-sm btn-secondary">View</button>
        <button class="btn btn-sm btn-secondary">Edit</button>
        <button class="btn btn-sm btn-danger">Delete</button>
      </div>
    </div>
  </div>
</div>
```

### Pattern 2: Form View (for vendor-form, PO-form, user-form, etc.)

```html
<div class="[module]-form-container">
  <div class="header">
    <h1>{{ isEditMode ? 'Edit' : 'Add New' }} Item</h1>
    <button class="btn btn-secondary">Back to List</button>
  </div>

  <div class="error-message" *ngIf="error">{{ error }}</div>

  <form [formGroup]="form" (ngSubmit)="onSubmit()">
    <div class="form-section">
      <h2>Section Title</h2>

      <div class="form-row">
        <div class="form-group">
          <label for="field">Field Label *</label>
          <input
            type="text"
            id="field"
            formControlName="field"
            [class.error]="submitted && f['field'].errors"
            placeholder="Enter value"
          />
          <span *ngIf="submitted && f['field'].errors" class="error-text">
            Error message
          </span>
        </div>
      </div>
    </div>

    <div class="form-actions">
      <button type="button" class="btn btn-secondary">Cancel</button>
      <button type="submit" class="btn btn-primary" [disabled]="loading">
        {{ loading ? 'Saving...' : 'Save' }}
      </button>
    </div>
  </form>
</div>
```

### Pattern 3: Report View (with summary cards + table)

```html
<div class="[report]-container">
  <div class="header">
    <h1>Report Title</h1>
    <div class="header-actions">
      <button class="btn btn-secondary">Export</button>
    </div>
  </div>

  <div class="summary-cards">
    <div class="summary-card">
      <h6>Metric Title</h6>
      <h3>{{ value }}</h3>
      <small>Additional info</small>
    </div>
  </div>

  <div class="filters">
    <div class="filter-row">
      <div class="form-group">
        <label>Date From</label>
        <input type="date" class="form-control" />
      </div>
      <div class="form-group">
        <button class="btn btn-primary">Generate Report</button>
      </div>
    </div>
  </div>

  <div class="card">
    <div class="card-header">
      <h5>Report Details</h5>
    </div>
    <div class="card-body">
      <div class="table-responsive">
        <table class="table table-hover">
          <!-- Table content -->
        </table>
      </div>
    </div>
  </div>
</div>
```

---

## Key Changes Summary

### From Old Design:
- ❌ `.container-fluid` wrapper
- ❌ Bootstrap row/col grid (`div.row > div.col-md-6`)
- ❌ `<h2>` for main titles
- ❌ Table-based list views for everything
- ❌ `class="mb-3"` Bootstrap spacing
- ❌ `.card > .card-body` for forms

### To New Design:
- ✅ Component-specific container (`.vendor-list-container`)
- ✅ CSS Grid for layouts (`.form-row`, `.items-grid`)
- ✅ `<h1>` with gradient for main titles
- ✅ Card-grid layouts for list views
- ✅ `.form-section` with `.form-row` for forms
- ✅ `.form-actions` bar at bottom
- ✅ Consistent `.info-row` for displaying data

---

## CSS Classes to Use

### Container
- `.{module}-list-container` or `.{module}-form-container`

### Header
- `.header` - Flex container for title and actions
- `.header h1` - Gradient title
- `.header-actions` - Button group

### Filters & Search
- `.filters` - White card wrapper
- `.search-box` - Flex container for search input and button
- `.search-input` - Search input field
- `.filter-row` - Grid layout for filter fields

### Grid Layouts
- `.items-grid` - Card grid container
- `.item-card` - Individual card
- `.item-header` - Card header with title
- `.item-info` - Card information section
- `.item-actions` - Card action buttons

### Forms
- `.form-section` - Form section with border
- `.form-row` - Grid row for form fields
- `.form-group` - Individual field container
- `.form-actions` - Bottom action bar

### Info Display
- `.info-row` - Flex row for label-value pairs
- `.label` - Label text
- `.value` - Value text

### States
- `.loading` - Loading message
- `.error-message` - Error message
- `.no-data` - No data message

---

## Next Steps

1. ✅ Update vendor-list and vendor-form HTML
2. 🔄 Update remaining Purchases module HTML templates
3. 🔄 Update Reports module HTML templates
4. 🔄 Update Users module HTML templates
5. ▶️ Run automation script to capture screenshots
6. ✅ Review and compare with Products reference
7. 🔧 Fix any inconsistencies found

---

## Testing Checklist

After all updates:
- [ ] All pages have gradient h1 headers
- [ ] All list pages use card-grid layout
- [ ] All forms use form-section layout
- [ ] All pages are responsive
- [ ] Loading/error states display correctly
- [ ] Action buttons are consistent
- [ ] Spacing and shadows are consistent
- [ ] Compare with Products reference screenshots

