# Commit Message Guidelines

This workspace uses [Conventional Commits](https://www.conventionalcommits.org/) format for commit messages.

## Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

## Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, missing semicolons, etc.)
- `refactor`: Code refactoring without feature changes or bug fixes
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `build`: Changes to build system or dependencies
- `ci`: Changes to CI configuration
- `chore`: Other changes that don't modify src or test files
- `revert`: Reverts a previous commit

## Scope

The scope should be one of:

- `chain` - Mystira.Chain repository
- `app` - Mystira.App repository
- `story-generator` - Mystira.StoryGenerator repository
- `infra` - Mystira.Infra repository
- `workspace` - Workspace configuration
- `deps` - Dependency updates

## Examples

### Good Commit Messages

```
feat(chain): add NFT minting contract
fix(app): resolve auth token refresh issue
docs(workspace): update installation instructions
chore(deps): update pnpm to 8.10.0
build(workspace): add pnpm lockfile for dependency management
```

### Bad Commit Messages

```
Add pnpm lockfile                    # Missing type and scope
fix: bug fix                          # Missing scope
feat: new feature                     # Missing scope
chore(workspace): add lockfile        # Subject should be more descriptive
```

## Subject Line

- Use imperative mood ("add" not "added" or "adds")
- Don't capitalize the first letter
- No period at the end
- Maximum 72 characters

## Body (Optional)

- Separate from subject with a blank line
- Explain what and why vs. how
- Can include multiple paragraphs
- Wrap at 72 characters

## Footer (Optional)

- Reference issues: `Fixes #123`, `Closes #456`
- Breaking changes: `BREAKING CHANGE: description`

## Quick Reference

For adding a pnpm lockfile:

```bash
git commit -m "build(workspace): add pnpm lockfile for dependency management"
```

For dependency updates:

```bash
git commit -m "chore(deps): update dependencies"
```

For workspace configuration:

```bash
git commit -m "chore(workspace): configure commitlint and husky"
```
