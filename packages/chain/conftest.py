"""Pytest configuration for Mystira.Chain tests."""


def pytest_addoption(parser):
    """Register custom CLI options passed by the monorepo test runner."""
    parser.addoption(
        "--coverage",
        action="store_true",
        default=False,
        help="Enable coverage reporting (passed by turbo; use pytest-cov --cov flags for configuration)",
    )
