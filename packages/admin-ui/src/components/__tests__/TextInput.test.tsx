import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import TextInput from "../TextInput";

describe("TextInput", () => {
  it("renders text input", () => {
    render(<TextInput />);
    const input = screen.getByRole("textbox");
    expect(input).toBeInTheDocument();
    expect(input).toHaveClass("form-control");
  });

  it("applies error styling when error prop is provided", () => {
    render(<TextInput error="Invalid input" />);
    const input = screen.getByRole("textbox");
    expect(input).toHaveClass("form-control", "is-invalid");
  });

  it("merges custom className with default classes", () => {
    render(<TextInput className="custom-class" />);
    const input = screen.getByRole("textbox");
    expect(input).toHaveClass("form-control", "custom-class");
  });

  it("passes through HTML attributes", async () => {
    const _user = userEvent.setup();
    render(<TextInput placeholder="Enter text" disabled />);
    const input = screen.getByPlaceholderText("Enter text");

    expect(input).toBeDisabled();
    expect(input).toHaveAttribute("type", "text");
  });

  it("handles user input", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<TextInput onChange={onChange} />);

    const input = screen.getByRole("textbox");
    await user.type(input, "hello");

    expect(onChange).toHaveBeenCalledTimes(5); // once for each character
  });

  it("supports ref forwarding", () => {
    const ref = { current: null };
    render(<TextInput ref={ref} />);

    expect(ref.current).toBeInstanceOf(HTMLInputElement);
  });
});
