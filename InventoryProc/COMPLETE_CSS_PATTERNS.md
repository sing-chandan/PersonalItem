# Complete CSS Patterns - Products Style

## Pattern 1: List/Grid View (For vendor-list, PO-list, user-list, etc.)

```css
/* Container */
.list-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
  background-color: #f5f7fa;
  min-height: 100vh;
}

/* Header with Gradient Title */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.header h1 {
  margin: 0;
  font-size: 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.header-actions {
  display: flex;
  gap: 1rem;
}

/* Search/Filters Section */
.filters {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.search-input {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 1rem;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
}

/* Grid Layout for Cards */
.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
}

/* Card Styling */
.item-card {
  position: relative;
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
}

.item-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.item-header {
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 2px solid #f0f0f0;
}

.item-header h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.25rem;
  color: #333;
}

.info-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.label {
  color: #666;
  font-size: 0.875rem;
}

.value {
  font-weight: 600;
  color: #333;
}

/* Actions in Card */
.item-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
}

/* Responsive */
@media (max-width: 768px) {
  .list-container {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .items-grid {
    grid-template-columns: 1fr;
  }
}
```

## Pattern 2: Form View (For vendor-form, PO-form, user-form, etc.)

```css
/* Container */
.form-container {
  padding: 2rem;
  max-width: 1000px;
  margin: 0 auto;
  background-color: #f5f7fa;
  min-height: 100vh;
}

/* Header */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.header h1 {
  margin: 0;
  font-size: 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* Form Card */
.form-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

/* Form Sections */
.form-section {
  padding: 2rem;
  border-bottom: 1px solid #e0e0e0;
}

.form-section:last-of-type {
  border-bottom: none;
}

.form-section h2 {
  margin: 0 0 1.5rem 0;
  font-size: 1.25rem;
  color: #333;
  font-weight: 600;
}

/* Grid Form Rows */
.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.form-row:last-child {
  margin-bottom: 0;
}

/* Form Groups */
.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
  font-size: 0.875rem;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 0.75rem;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #667eea;
}

/* Form Actions Bar */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem 2rem;
  background: #f9f9f9;
}

/* Responsive */
@media (max-width: 768px) {
  .form-container {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .form-section {
    padding: 1.5rem;
  }

  .form-row {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .form-actions .btn {
    width: 100%;
  }
}
```

## Pattern 3: Buttons (Consistent across all pages)

```css
.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.btn-secondary {
  background: #f0f0f0;
  color: #333;
}

.btn-secondary:not(:disabled):hover {
  background: #e0e0e0;
}

.btn-danger {
  background: #f44336;
  color: white;
}

.btn-danger:not(:disabled):hover {
  background: #d32f2f;
}

.btn-sm {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
}
```

---

## How to Apply:

1. **For List Components** (vendor-list, PO-list, GRN-list, user-list, reports):
   - Use Pattern 1 (List/Grid View)
   - Replace table-based layout with `.items-grid`
   - Use `.item-card` for each item

2. **For Form Components** (vendor-form, PO-form, user-form):
   - Use Pattern 2 (Form View)
   - Use `.form-row` with grid for form fields
   - Add `.form-actions` bar at bottom

3. **For All Components**:
   - Use Pattern 3 for buttons
   - Apply gradient to h1 headers
   - Use white cards with shadows
   - Implement responsive breakpoints

## Key Benefits:
✅ Consistent design across all modules
✅ Responsive grid layouts
✅ Better mobile experience
✅ Modern card-based UI
✅ Improved visual hierarchy
✅ Better spacing and readability
