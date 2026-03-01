import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import LoadingSpinner from "../LoadingSpinner";

describe("LoadingSpinner", () => {
  it("renders loading spinner", () => {
    render(<LoadingSpinner />);
    const spinner = screen.getByRole("status");
    expect(spinner).toBeInTheDocument();
    expect(spinner).toHaveClass("spinner-border");
  });

  it("displays loading message", () => {
    render(<LoadingSpinner message="Loading data..." />);
    const message = screen.getByText("Loading data...", {
      selector: ".text-muted",
    });
    expect(message).toBeInTheDocument();
  });

  it("applies small size class", () => {
    render(<LoadingSpinner size="sm" />);
    const spinner = screen.getByRole("status");
    expect(spinner).toHaveClass("spinner-border-sm");
  });

  it("applies large size class", () => {
    render(<LoadingSpinner size="lg" />);
    const spinner = screen.getByRole("status");
    expect(spinner).toHaveClass("spinner-border");
    expect(spinner).not.toHaveClass("spinner-border-sm");
  });
});
