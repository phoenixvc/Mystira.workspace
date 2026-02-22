# Testing Checklist - Mystira Admin UI

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2024-12-22
**Author**: Development Team

## Overview

This checklist ensures comprehensive testing of the Mystira Admin UI before deployment. Each section must be completed and signed off before proceeding to production.

---

## Pre-Deployment Testing

### Build Validation

- [ ] **npm install**: All dependencies install without errors
  ```bash
  rm -rf node_modules package-lock.json
  npm install
  ```

- [ ] **npm run build**: Production build succeeds
  ```bash
  npm run build
  ```

- [ ] **npm run lint**: No linting errors
  ```bash
  npm run lint
  ```

- [ ] **TypeScript**: No type errors
  ```bash
  npx tsc --noEmit
  ```

- [ ] **Bundle Size**: Under acceptable limits
  ```bash
  npm run build
  du -sh dist/
  # Expected: < 5MB uncompressed
  ```

### Environment Configuration

- [ ] **Environment Variables**: All required variables set
  ```bash
  # Required:
  VITE_API_BASE_URL=<admin-api-url>
  ```

- [ ] **API URL**: Correct API endpoint configured
- [ ] **No Hardcoded URLs**: All URLs from environment variables

---

## Functional Testing

### Authentication

- [ ] **Login Flow**:
  - [ ] Login page loads correctly
  - [ ] Valid credentials allow login
  - [ ] Invalid credentials show error message
  - [ ] Password field is masked
  - [ ] Login redirects to dashboard

- [ ] **Session Management**:
  - [ ] Session persists on page refresh
  - [ ] Session expires after timeout
  - [ ] Logout clears session
  - [ ] Protected routes redirect to login when unauthenticated

- [ ] **Authorization**:
  - [ ] Unauthorized users cannot access admin pages
  - [ ] Role-based access (if applicable)

### Dashboard

- [ ] **Dashboard Page**:
  - [ ] Page loads successfully
  - [ ] Statistics display correctly
  - [ ] Recent items show
  - [ ] Quick actions work
  - [ ] Refresh button updates data

### Scenarios

- [ ] **List Page**:
  - [ ] Scenarios load and display
  - [ ] Search filters results
  - [ ] Pagination works
  - [ ] Empty state displays when no results

- [ ] **Create Scenario**:
  - [ ] Form validates required fields
  - [ ] Submit creates scenario
  - [ ] Success toast appears
  - [ ] Redirects to list after create

- [ ] **Edit Scenario**:
  - [ ] Form loads with existing data
  - [ ] Changes save correctly
  - [ ] Success toast appears

- [ ] **Delete Scenario**:
  - [ ] Confirmation dialog appears
  - [ ] Delete removes scenario
  - [ ] Success toast appears

- [ ] **Import Scenario**:
  - [ ] File upload works
  - [ ] Validation errors display
  - [ ] Success message appears

### Media

- [ ] **List Page**:
  - [ ] Media items load and display
  - [ ] Search works
  - [ ] Pagination works

- [ ] **Upload Media**:
  - [ ] File selection works
  - [ ] Upload succeeds
  - [ ] Preview displays (for images)

- [ ] **Delete Media**:
  - [ ] Confirmation appears
  - [ ] Delete works

### Badges

- [ ] **List Page**: Load, search, pagination work
- [ ] **Create Badge**: Form validates, creates badge
- [ ] **Edit Badge**: Loads data, saves changes
- [ ] **Delete Badge**: Confirms and deletes
- [ ] **Import Badge**: File upload with image works

### Bundles

- [ ] **List Page**: Load, search, pagination work
- [ ] **Import Bundle**: File upload with options works

### Character Maps

- [ ] **List Page**: Load, search, pagination work
- [ ] **Create Character Map**: Form validates, creates
- [ ] **Edit Character Map**: Loads data, saves changes
- [ ] **Delete Character Map**: Confirms and deletes
- [ ] **Import Character Map**: File upload works

### Master Data (All Types)

For each type (Age Groups, Archetypes, Compass Axes, Echo Types, Fantasy Themes):

- [ ] **List Page**: Load, search, pagination work
- [ ] **Create**: Form validates, creates item
- [ ] **Edit**: Loads data, saves changes
- [ ] **Delete**: Confirms and deletes

---

## UI/UX Testing

### Layout & Navigation

- [ ] **Navigation**: All menu items work
- [ ] **Breadcrumbs**: Display correctly (if applicable)
- [ ] **Active State**: Current page highlighted in nav
- [ ] **Logo**: Links to dashboard

### Components

- [ ] **LoadingSpinner**: Shows during data fetch
- [ ] **ErrorAlert**: Shows on errors with retry button
- [ ] **SearchBar**: Debounces input, filters results
- [ ] **Pagination**: Shows correct page info, navigation works
- [ ] **Toasts**: Appear and dismiss correctly
- [ ] **ConfirmationDialog**: Shows and handles confirm/cancel

### Forms

- [ ] **Validation**: Error messages show for invalid fields
- [ ] **Required Fields**: Asterisk or indicator shown
- [ ] **Submit Button**: Disabled during submission
- [ ] **Loading State**: Shows during submit

### Responsive Design

- [ ] **Desktop (1920px)**: Layout correct
- [ ] **Laptop (1366px)**: Layout correct
- [ ] **Tablet (768px)**: Layout adapts
- [ ] **Mobile (375px)**: Layout adapts, usable

---

## Browser Compatibility

- [ ] **Chrome (latest)**: All features work
- [ ] **Firefox (latest)**: All features work
- [ ] **Safari (latest)**: All features work
- [ ] **Edge (latest)**: All features work

---

## Performance Testing

- [ ] **Initial Load Time**: < 3 seconds on 3G
- [ ] **Time to Interactive**: < 5 seconds
- [ ] **Lighthouse Score**: > 80 for Performance
- [ ] **No Console Errors**: Clean console in production

### Metrics to Capture

| Metric | Target | Actual |
|--------|--------|--------|
| First Contentful Paint | < 1.5s | _____ |
| Time to Interactive | < 3.0s | _____ |
| Lighthouse Performance | > 80 | _____ |
| Bundle Size (gzip) | < 500KB | _____ |

---

## API Integration Testing

### Connectivity

- [ ] **API Reachable**: Can connect to Admin API
- [ ] **CORS**: No CORS errors in browser
- [ ] **SSL**: HTTPS works correctly

### Error Handling

- [ ] **Network Error**: Displays friendly message
- [ ] **401 Unauthorized**: Redirects to login
- [ ] **403 Forbidden**: Shows access denied
- [ ] **404 Not Found**: Shows not found message
- [ ] **500 Server Error**: Shows error with retry

### Data Integrity

- [ ] **Create**: New items appear in list
- [ ] **Update**: Changes reflect immediately
- [ ] **Delete**: Items removed from list
- [ ] **Refresh**: Data stays consistent

---

## Security Testing

- [ ] **No Sensitive Data in Console**: No tokens/passwords logged
- [ ] **No Sensitive Data in Storage**: LocalStorage reviewed
- [ ] **XSS**: No script injection vulnerabilities
- [ ] **CSRF**: Protected (cookies with SameSite)

---

## Accessibility Testing

- [ ] **Keyboard Navigation**: All features keyboard accessible
- [ ] **Tab Order**: Logical tab sequence
- [ ] **Focus Indicators**: Visible focus states
- [ ] **Alt Text**: Images have alt attributes
- [ ] **Form Labels**: All inputs have labels
- [ ] **Color Contrast**: WCAG AA compliant

---

## Test Environments

### Development
- [ ] All tests pass in local development

### Staging
- [ ] All tests pass against staging API
- [ ] Real data scenarios tested

### Production (Smoke Tests)
- [ ] Login works
- [ ] Dashboard loads
- [ ] Navigation works
- [ ] One CRUD operation per entity type

---

## Sign-Off

### Pre-Production Sign-Off

**Tested By**: _________________
**Date**: _________________
**Signature**: _________________

### Production Release Sign-Off

**Approved By**: _________________
**Date**: _________________
**Signature**: _________________

---

## Notes Section

Use this section to document any issues found during testing:

```
Date: ___________
Test: ___________
Issue:



Resolution:



Tester: ___________
```

---

## References

- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md)
- [Rollback Procedure](./ROLLBACK_PROCEDURE.md)
- [Implementation Checklist](../planning/implementation-checklist.md)
