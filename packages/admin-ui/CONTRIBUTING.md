# Contributing to Mystira Admin UI

Thank you for your interest in contributing to Mystira Admin UI! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on what is best for the project
- Show empathy towards other contributors
- Accept constructive criticism gracefully

### Unacceptable Behavior

- Harassment or discriminatory language
- Personal attacks or trolling
- Publishing private information
- Unprofessional conduct

## Getting Started

### Prerequisites

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:

```bash
git clone https://github.com/YOUR_USERNAME/Mystira.Admin.UI.git
cd Mystira.Admin.UI
```

3. **Add upstream remote**:

```bash
git remote add upstream https://github.com/phoenixvc/Mystira.Admin.UI.git
```

4. **Install dependencies**:

```bash
pnpm install
```

5. **Create a branch** for your work:

```bash
git checkout -b feature/your-feature-name
```

### Development Setup

1. Copy `.env.example` to `.env.local` and configure
2. Start the development server: `pnpm dev`
3. Open `http://localhost:7001` in your browser

## Development Workflow

### Branch Naming

Use descriptive branch names with prefixes:

- `feature/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation changes
- `test/` - Test additions or modifications
- `chore/` - Maintenance tasks

**Examples:**
- `feature/add-user-settings`
- `fix/scenario-validation-error`
- `refactor/extract-validation-hook`
- `docs/update-api-documentation`

### Keeping Your Branch Updated

Regularly sync with upstream:

```bash
git fetch upstream
git rebase upstream/dev
```

### Making Changes

1. **Make your changes** in your feature branch
2. **Follow coding standards** (see below)
3. **Write tests** for new functionality
4. **Update documentation** if needed
5. **Run checks** before committing:

```bash
pnpm typecheck
pnpm lint
pnpm test
```

## Coding Standards

### TypeScript

- **Use strict types**: No `any` types unless absolutely necessary
- **Define interfaces**: Create interfaces for all props and data structures
- **Use type inference**: Let TypeScript infer types when obvious
- **Avoid type assertions**: Use type guards instead

**Good:**
```tsx
interface UserProps {
  name: string;
  age: number;
  email?: string;
}

function User({ name, age, email }: UserProps) {
  // Component logic
}
```

**Bad:**
```tsx
function User(props: any) {
  // Avoid any types
}
```

### React Components

- **Functional components**: Use function components with hooks
- **Named exports**: Prefer named exports over default exports
- **Props destructuring**: Destructure props in function signature
- **Hooks order**: Follow React hooks rules

**Good:**
```tsx
interface ButtonProps {
  label: string;
  onClick: () => void;
  disabled?: boolean;
}

export function Button({ label, onClick, disabled = false }: ButtonProps) {
  return (
    <button onClick={onClick} disabled={disabled}>
      {label}
    </button>
  );
}
```

### File Organization

- **One component per file**: Each component in its own file
- **Co-locate tests**: Place test files next to components
- **Group related files**: Keep related components together
- **Use index files**: Export multiple items from directories

**Structure:**
```
components/
├── Button/
│   ├── Button.tsx
│   ├── Button.test.tsx
│   └── index.ts
```

### Naming Conventions

- **Components**: PascalCase (e.g., `UserProfile`, `MediaCard`)
- **Functions**: camelCase (e.g., `getUserData`, `validateForm`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `API_BASE_URL`, `MAX_RETRIES`)
- **Files**: Match component name (e.g., `UserProfile.tsx`)
- **CSS classes**: kebab-case (e.g., `user-profile`, `media-card`)

### Styling

- **Bootstrap first**: Use Bootstrap classes when possible
- **Custom CSS**: Add custom styles only when needed
- **CSS Modules**: Use CSS modules for component-specific styles
- **Responsive**: Ensure mobile-friendly design

### Error Handling

- **Use error boundaries**: Wrap components that might error
- **Handle API errors**: Use `handleApiError` utility
- **User-friendly messages**: Show clear error messages to users
- **Log for debugging**: Console log errors in development

**Example:**
```tsx
import { handleApiError } from '../utils/errorHandler';

try {
  await api.createScenario(data);
  showToast.success('Scenario created successfully');
} catch (error) {
  handleApiError(error, 'Failed to create scenario');
}
```

### State Management

- **Local state**: Use `useState` for component-local state
- **Global state**: Use Zustand stores for app-wide state
- **Server state**: Use React Query for API data
- **Form state**: Use React Hook Form for forms

### Performance

- **Memoization**: Use `useMemo` and `useCallback` when appropriate
- **Lazy loading**: Code-split large components
- **Avoid re-renders**: Optimize component re-rendering
- **Debounce inputs**: Debounce search and filter inputs

## Commit Guidelines

### Commit Message Format

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements

### Examples

```
feat(scenarios): add schema validation for scenario imports

- Implement client-side validation using Ajv
- Add ValidationResults component
- Display detailed error messages
- Support YAML and JSON files

Closes #123
```

```
fix(media): resolve ZIP upload metadata parsing issue

The media-metadata.json file was not being parsed correctly
when uploaded via ZIP. This fix ensures proper JSON parsing
and error handling.

Fixes #456
```

### Commit Best Practices

- **Atomic commits**: One logical change per commit
- **Clear messages**: Describe what and why, not how
- **Reference issues**: Link to related issues
- **Sign commits**: Use GPG signing if possible

## Pull Request Process

### Before Submitting

1. **Sync with upstream**: Rebase on latest `dev` branch
2. **Run all checks**: Ensure tests, linting, and type checking pass
3. **Update documentation**: Add or update relevant docs
4. **Test manually**: Verify changes work as expected

### Creating a Pull Request

1. **Push your branch** to your fork:

```bash
git push origin feature/your-feature-name
```

2. **Open a pull request** on GitHub
3. **Fill out the PR template** completely
4. **Link related issues** using keywords (Closes #123)
5. **Request review** from maintainers

### PR Title Format

Use the same format as commit messages:

```
feat(scenarios): add schema validation for scenario imports
```

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing
- [ ] Unit tests added/updated
- [ ] Manual testing completed
- [ ] All tests passing

## Screenshots (if applicable)
Add screenshots here

## Related Issues
Closes #123
```

### Review Process

1. **Automated checks**: CI/CD pipeline must pass
2. **Code review**: At least one approval required
3. **Address feedback**: Make requested changes
4. **Re-request review**: After addressing feedback
5. **Merge**: Maintainer will merge when approved

### After Merge

1. **Delete your branch**: Clean up after merge
2. **Sync your fork**: Pull latest changes
3. **Close related issues**: Ensure issues are closed

## Testing Guidelines

### Test Coverage

- **Unit tests**: Test individual functions and components
- **Integration tests**: Test component interactions
- **E2E tests**: Test complete user flows (future)

### Writing Tests

Use React Testing Library and Vitest:

```tsx
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Button } from './Button';

describe('Button', () => {
  it('renders with label', () => {
    render(<Button label="Click me" onClick={() => {}} />);
    expect(screen.getByText('Click me')).toBeInTheDocument();
  });

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn();
    render(<Button label="Click me" onClick={handleClick} />);
    
    fireEvent.click(screen.getByText('Click me'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('is disabled when disabled prop is true', () => {
    render(<Button label="Click me" onClick={() => {}} disabled />);
    expect(screen.getByText('Click me')).toBeDisabled();
  });
});
```

### Test Best Practices

- **Test behavior**: Test what users see and do
- **Avoid implementation details**: Don't test internal state
- **Use meaningful assertions**: Clear expectations
- **Mock external dependencies**: Isolate unit tests
- **Test edge cases**: Cover error scenarios

## Documentation

### Code Documentation

- **JSDoc comments**: Document complex functions
- **Type definitions**: Use TypeScript types as documentation
- **README updates**: Keep README current
- **Inline comments**: Explain non-obvious code

### Documentation Standards

- **Clear and concise**: Write for clarity
- **Examples**: Provide code examples
- **Up-to-date**: Update docs with code changes
- **Markdown**: Use proper Markdown formatting

### What to Document

- **New features**: How to use them
- **API changes**: Breaking changes and migrations
- **Configuration**: New environment variables
- **Architecture**: Significant design decisions

## Questions?

If you have questions or need help:

- **GitHub Discussions**: Ask questions in discussions
- **GitHub Issues**: Report bugs or request features
- **Code Review**: Ask during PR review

Thank you for contributing to Mystira Admin UI! 🎉
