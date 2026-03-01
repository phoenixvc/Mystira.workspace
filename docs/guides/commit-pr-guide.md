# Commit and Pull Request Guide

**Version**: 1.0  
**Last Updated**: 2026-03-01  
**Purpose**: Standardize commit messages and PR processes for the Mystira monorepo

## Overview

This guide establishes standards for creating clear, consistent commit messages and well-structured pull requests. Following these guidelines ensures better code review, easier maintenance, and comprehensive historical documentation.

## Commit Message Standards

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type       | Description   | When to Use                |
| ---------- | ------------- | -------------------------- |
| `feat`     | New feature   | User-facing functionality  |
| `fix`      | Bug fix       | Resolving issues           |
| `docs`     | Documentation | Documentation changes only |
| `style`    | Code style    | Formatting, linting        |
| `refactor` | Refactoring   | Code structure changes     |
| `perf`     | Performance   | Performance improvements   |
| `test`     | Tests         | Adding/modifying tests     |
| `build`    | Build         | Build system changes       |
| `ci`       | CI/CD         | Pipeline changes           |
| `chore`    | Chores        | Maintenance tasks          |
| `revert`   | Revert        | Reverting changes          |

### Scopes

| Scope             | Description           | Examples                               |
| ----------------- | --------------------- | -------------------------------------- |
| `chain`           | Blockchain components | Smart contracts, chain utilities       |
| `app`             | Main application      | App API, PWA, infrastructure           |
| `story-generator` | Story generator       | API, web, domain, LLM                  |
| `infra`           | Infrastructure        | Shared infrastructure components       |
| `workspace`       | Monorepo              | Build tools, configuration             |
| `deps`            | Dependencies          | Package updates, dependency management |

### Subject Line

- **Imperative mood**: Use "add", "fix", "update", not "added", "fixed", "updated"
- **Lowercase**: All lowercase
- **Concise**: Maximum 50 characters
- **Descriptive**: What the change does, not how

**Good Examples**:

```
feat(app): add user authentication system
fix(story-generator): resolve null reference in LLM service
refactor(infra): consolidate WhatsApp infrastructure
docs(workspace): update PR documentation guide
```

**Bad Examples**:

```
Fixed bug in authentication
Added new feature for users
Update stuff
Code changes
```

### Body (Optional but Recommended)

- **One blank line** after subject
- **Wrap at 72 characters**
- **Explain what and why**, not how
- **Use imperative mood**
- **Reference issues**: `Closes #123`, `Fixes #456`

**Example**:

```
Add user authentication system with JWT tokens and refresh
mechanism. This enables secure user sessions and improves
overall security posture.

Closes #123
Fixes #456
```

### Footer

- **One blank line** after body
- **Reference issues**: `Closes #123`, `Related to #456`
- **Co-authors**: `Co-authored-by: Name <email>`
- **Breaking changes**: `BREAKING CHANGE:` description

## Documentation Requirements

### When Documentation is Required

#### ✅ **Required** (High Impact)

- **Architecture Decisions**: ADR implementations
- **Major Refactoring**: Affects >5 projects or >10k lines
- **New Features**: Significant user-facing functionality
- **Performance Improvements**: Major optimizations
- **Security Changes**: Authentication, authorization fixes

#### 🔄 **Recommended** (Medium Impact)

- **Complex Bug Fixes**: Critical or high-severity issues
- **Library Upgrades**: Major version migrations
- **Test Improvements**: Significant coverage changes

#### ⚪ **Optional** (Low Impact)

- **Minor Features**: Small enhancements
- **Simple Bug Fixes**: Straightforward fixes
- **Configuration Changes**: Minor updates

### Creating Documentation

1. **Generate Template**:

   ```bash
   ./scripts/create-doc.ps1 <type> <title> <pr>
   ```

2. **Fill Template**: Complete all `[bracketed]` sections

3. **Validate**:

   ```bash
   ./scripts/validate-documentation.ps1 <file-path>
   ```

4. **Link in PR**: Add documentation link to PR description

### Documentation Checklist

- [ ] Documentation created (if required)
- [ ] Template filled completely
- [ ] Sequential number assigned
- [ ] Technical review completed
- [ ] Peer review completed
- [ ] Linked to related ADRs
- [ ] Filed in correct location

## Pull Request Standards

### PR Title Format

Follow commit message format for PR title:

```
<type>(<scope>): <subject>
```

### PR Description Template

````markdown
## 📋 Description

[Brief description of what this PR accomplishes]

## 🎯 Objectives

- [ ] Objective 1
- [ ] Objective 2
- [ ] Objective 3

## 🔄 Changes Made

### Added

- [ ] Feature/Component 1
- [ ] Feature/Component 2

### Modified

- [ ] Modified Component 1
- [ ] Modified Component 2

### Removed

- [ ] Removed Component 1
- [ ] Removed Component 2

### Fixed

- [ ] Bug fix 1
- [ ] Bug fix 2

## 🧪 Testing

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] Performance testing (if applicable)

## 📚 Documentation

- [ ] Code comments updated
- [ ] API documentation updated
- [ ] User documentation updated
- [ ] Historical documentation created (if required)

### Documentation Link

[Link to created documentation file]

## 🔗 Related Issues

- Closes #123
- Related to #456
- Depends on #789

## 📸 Screenshots/Videos

[Add screenshots or videos for UI changes]

## ⚠️ Breaking Changes

[Describe any breaking changes and migration steps]

## 🚀 Deployment Notes

[Any special deployment instructions or considerations]

## 📝 Review Checklist

### Code Quality

- [ ] Code follows project standards
- [ ] No TODO/FIXME comments left
- [ ] Error handling implemented
- [ ] Logging added where appropriate

### Testing

- [ ] Tests cover new functionality
- [ ] Tests cover edge cases
- [ ] Tests are well-structured
- [ ] No flaky tests introduced

### Documentation

- [ ] Commit messages are clear
- [ ] PR description is comprehensive
- [ ] Documentation is accurate
- [ ] Examples are provided

### Performance

- [ ] No performance regressions
- [ ] Database queries optimized
- [ ] Memory usage acceptable
- [ ] Load testing completed (if applicable)

### Security

- [ ] No security vulnerabilities
- [ ] Input validation implemented
- [ ] Authentication/authorization considered
- [ ] Sensitive data protected

## 📊 Impact Assessment

### Scope

- [ ] **High**: Affects >5 projects or >10k lines
- [ ] **Medium**: Affects 2-5 projects or 1-10k lines
- [ ] **Low**: Affects <2 projects or <1k lines

### Risk Level

- [ ] **High**: Breaking changes or security implications
- [ ] **Medium**: Significant functionality changes
- [ ] **Low**: Minor improvements or fixes

## 🔄 Review Process

1. **Self-Review**: Review your own changes first
2. **Technical Review**: Request review from technical expert
3. **Peer Review**: Request review from team member
4. **Approval**: Get approval from required reviewers
5. **Merge**: Merge after all requirements met

## 🚀 Merge Requirements

### Before Merging

- [ ] All CI checks pass
- [ ] All discussions resolved
- [ ] Required approvals received
- [ ] Documentation completed
- [ ] Testing completed
- [ ] Breaking changes documented

### Merge Strategy

- **Squash and Merge**: For most PRs (clean history)
- **Merge Commit**: For complex multi-author PRs
- **Rebase and Merge**: For long-running feature branches

## 📝 Best Practices

### Commits

- **Atomic**: One logical change per commit
- **Testable**: Each commit should build and test
- **Clear**: Descriptive commit messages
- **Consistent**: Follow established patterns

### PRs

- **Focused**: Single feature or fix per PR
- **Complete**: All related changes included
- **Tested**: Comprehensive test coverage
- **Documented**: Clear description and documentation

### Code Review

- **Constructive**: Helpful, respectful feedback
- **Thorough**: Check logic, performance, security
- **Timely**: Respond to reviews promptly
- **Collaborative**: Work together to improve code

## 🔧 Tools and Automation

### Pre-commit Hooks

```bash
# Install husky for pre-commit hooks
npm install --save-dev husky

# Pre-commit validation runs automatically
npx husky add .husky/pre-commit "./scripts/pre-commit-check.sh"
```
````

### Commit Message Validation

```bash
# Validate commit message format
./scripts/validate-commit-message.sh
```

### Documentation Validation

```bash
# Validate documentation structure
./scripts/validate-documentation.ps1 <file-path>
```

### PR Template

PR template automatically enforces:

- Documentation requirements
- Testing checklist
- Review process
- Impact assessment

## 🚨 Common Issues and Solutions

### Commit Message Issues

**Problem**: Commit message doesn't follow format
**Solution**: Use commit message template and validation script

### Documentation Missing

**Problem**: Required documentation not created
**Solution**: Use `./scripts/create-doc.ps1` to generate template

### CI Failures

**Problem**: Tests failing in CI
**Solution**: Run tests locally before pushing, check environment differences

### Merge Conflicts

**Problem**: Merge conflicts in PR
**Solution**: Rebase feature branch, resolve conflicts locally

### Large PRs

**Problem**: PR too large to review effectively
**Solution:**

1. Split into smaller, focused PRs
2. Use feature flags for partial deployment
3. Provide comprehensive testing strategy

## 📚 Additional Resources

### Internal Documentation

- [PR Documentation Strategy](../process/pr-documentation-strategy.md)
- [Architecture Decision Records](../adr/)
- [Test Project Organization](../adr/test-project-analysis.md)

### External Resources

- [Conventional Commits](https://www.conventionalcommits.org/)
- [GitHub PR Guidelines](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests)
- [Effective Code Review](https://google.github.io/eng-practices/review/)

### Scripts and Tools

- `./scripts/create-doc.ps1` - Generate documentation templates
- `./scripts/validate-documentation.ps1` - Validate documentation structure
- `./scripts/validate-commit-message.sh` - Validate commit messages
- `./scripts/pre-commit-check.sh` - Pre-commit validation

## 🔄 Evolution of This Guide

This guide evolves based on:

- Team feedback and experience
- Changes in project structure
- New tools and automation
- Lessons learned from PRs

### Contributing to This Guide

1. Propose changes via PR
2. Get team consensus
3. Update examples and templates
4. Communicate changes to team

---

**Maintainers**: Development Team  
**Review Schedule**: Quarterly  
**Last Review**: 2026-03-01
