# ai-gateway: Request-to-Token Attribution (Implementation Plan)

Priority: P1 (High)

Goal: enable defensible incident attribution and cost accountability by correlating upstream requests to per-request token telemetry.

## Current state (ai-gateway)

- LiteLLM Gateway on Azure Container Apps
- JSON logging to Log Analytics (exists)
- Prometheus metrics at `/metrics` endpoint (aggregate only)
- No correlation IDs
- No per-request token telemetry

## Architecture (ai-gateway)

- Gateway: LiteLLM on Azure Container Apps
- State service: FastAPI with Redis
- Dashboard: Node.js admin UI
- Infrastructure: Terraform with Azure resources
- Logging: JSON logs to Log Analytics workspace
- Observability: Prometheus metrics endpoint

## Phase 1: OpenTelemetry integration (core)

Use LiteLLM’s built-in OpenTelemetry callback support (avoid custom LiteLLM images initially).

Planned changes (ai-gateway repo):

- Add `otel` to LiteLLM `success_callback` and `failure_callback` (Terraform)
- Add OTEL env vars:
  - `OTEL_SERVICE_NAME`
  - `OTEL_TRACER_NAME`
  - `OTEL_EXPORTER_OTLP_ENDPOINT`
  - `OTEL_EXPORTER_OTLP_PROTOCOL` (http/json)
- Add Terraform variables:
  - `otel_exporter_endpoint`
  - `otel_service_name`

Expected telemetry emitted by LiteLLM OTEL callback:

- Model name, provider, deployment
- Token usage (`prompt_tokens`, `completion_tokens`, `total_tokens`)
- Duration
- Request/response metadata

Collector requirement:

- An OTLP collector endpoint must exist (dedicated collector service, sidecar, or direct-to-backend if supported).

## Phase 2: Correlation ID propagation

Status: in progress (design)

### Method A: request metadata (recommended)

Upstream callers include correlation fields in the request body `metadata`:

```json
{
  "model": "gpt-5.3-codex",
  "messages": [{ "role": "user", "content": "Hello" }],
  "metadata": {
    "request_id": "req_123",
    "session_id": "sess_456",
    "workflow": "manual_orchestration",
    "stage": "writer",
    "endpoint": "/api/manual-orchestration/sessions/start",
    "user_id": "user_abc"
  }
}
```

LiteLLM propagates `metadata` into OTEL spans, enabling correlation in traces.

### Method B: HTTP headers (future enhancement)

For clients that can only send headers:

- `x-request-id`
- `x-session-id`
- `x-correlation-id`
- `x-workflow-name`
- `x-stage-name`
- `x-user-id`

This requires additional LiteLLM configuration or middleware (custom wrapper or sidecar).

## Phase 3: Per-request rollup

Options:

- Compute rollups in analytics (preferred): aggregate spans/events by `request_id` and `operation_id`.
- Compute rollups in ai-gateway: track tokens per `request_id` (memory/Redis) and emit a summary event on completion.

## Required upstream work (Mystira as upstream caller)

Mystira services that call ai-gateway must pass correlation fields (prefer Method A via `metadata`).

Primary requirement:

- Always populate `request_id`, `workflow`, `stage`, and `endpoint`.

## Downstream analytics (pvc-costops-analytics)

KQL queries/dashboards to:

- Join requests ↔ token events by `operation_Id` / `request_id`
- Aggregate by endpoint/workflow/stage/model/deployment

## Event shape (target)

```json
{
  "timestamp": "2026-03-01T12:34:56Z",
  "request_id": "req_123",
  "session_id": "sess_456",
  "endpoint": "/api/manual-orchestration/sessions/start",
  "workflow": "manual_orchestration",
  "stage": "writer",
  "provider": "anthropic",
  "model": "claude-sonnet-4-5",
  "deployment": "route-a",
  "prompt_tokens": 4200,
  "completion_tokens": 2100,
  "total_tokens": 6300,
  "duration_ms": 18750
}
```

## Acceptance criteria

- 100% of LLM calls emit token telemetry with `request_id` + `operation_id`
- 100% include `workflow` + `stage`
- Provide request-completion rollup totals (`total_tokens`, `llm_calls`)
- Support KQL joins requests↔token events by `operation_Id` / `request_id`

## Risks / decisions

- OTLP collector: decide where it runs and how it authenticates to the backend.
- Plan consistency: built-in OTEL avoids custom images; if token fields are incomplete, switch to a custom LiteLLM image or sidecar.
