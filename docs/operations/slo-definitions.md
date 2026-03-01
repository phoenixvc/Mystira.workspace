# Service Level Objectives (SLO) Definitions

**Status**: Active
**Version**: 1.0
**Last Updated**: 2025-12-22
**Owner**: Platform Engineering Team

## Overview

This document defines the Service Level Objectives (SLOs), Service Level Indicators (SLIs), and error budgets for the Mystira platform. These metrics drive our reliability engineering practices and inform operational decisions.

## Service Tier Definitions

| Tier | Services | Availability Target | Error Budget (monthly) |
|------|----------|---------------------|------------------------|
| **Tier 1 (Critical)** | App API, Story Generator | 99.9% | 43.2 minutes |
| **Tier 2 (High)** | Publisher, Chain | 99.5% | 3.6 hours |
| **Tier 3 (Standard)** | Admin API, DevHub | 99.0% | 7.2 hours |

---

## Tier 1 Services - Critical

### Mystira App API

**Service Description**: Core API serving user requests for gameplay, account management, and scenario interactions.

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.9% | 30-day rolling |
| **Error Budget** | 43.2 minutes/month | Monthly reset |

**SLI Formula**:
```
Availability = (Total Requests - 5xx Errors) / Total Requests × 100
```

**Measurement**:
- Source: Application Insights / Azure Monitor
- Query interval: 1 minute
- Aggregation: 30-day rolling average

#### Latency SLO

| Percentile | Target | Endpoint Category |
|------------|--------|-------------------|
| **p50** | < 100ms | Read operations |
| **p95** | < 500ms | Read operations |
| **p99** | < 1000ms | Read operations |
| **p50** | < 200ms | Write operations |
| **p95** | < 750ms | Write operations |
| **p99** | < 1500ms | Write operations |

**SLI Formula**:
```
Latency_P95 = Percentile(request_duration, 95)
```

**Measurement**:
- Source: Application Insights request telemetry
- Exclude: Health check endpoints, warming requests
- Include: All user-facing API endpoints

#### Throughput SLO

| Metric | Target | Notes |
|--------|--------|-------|
| **Requests/sec** | 1000 rps sustained | Normal operation |
| **Burst capacity** | 3000 rps (60 seconds) | Peak handling |

### Story Generator Service

**Service Description**: AI-powered story generation engine providing narrative content.

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.9% | 30-day rolling |
| **Error Budget** | 43.2 minutes/month | Monthly reset |

#### Latency SLO

| Percentile | Target | Operation Type |
|------------|--------|----------------|
| **p50** | < 2000ms | Story generation |
| **p95** | < 5000ms | Story generation |
| **p99** | < 10000ms | Story generation |
| **p50** | < 500ms | Choice evaluation |
| **p95** | < 1500ms | Choice evaluation |

**Notes**: Story generation latency depends on AI model response times. Targets account for external AI service dependencies.

---

## Tier 2 Services - High Priority

### Publisher Service

**Service Description**: Content publishing and distribution service.

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.5% | 30-day rolling |
| **Error Budget** | 3.6 hours/month | Monthly reset |

#### Latency SLO

| Percentile | Target | Operation |
|------------|--------|-----------|
| **p50** | < 200ms | Content retrieval |
| **p95** | < 800ms | Content retrieval |
| **p50** | < 500ms | Content publishing |
| **p95** | < 2000ms | Content publishing |

### Chain Service

**Service Description**: Blockchain integration service for NFT and token operations.

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.5% | 30-day rolling |
| **Error Budget** | 3.6 hours/month | Monthly reset |

#### Latency SLO

| Percentile | Target | Operation |
|------------|--------|-----------|
| **p50** | < 1000ms | Read operations |
| **p95** | < 3000ms | Read operations |
| **p50** | < 5000ms | Write operations (blockchain tx) |
| **p95** | < 15000ms | Write operations (blockchain tx) |

**Notes**: Blockchain write operations include confirmation time.

---

## Tier 3 Services - Standard

### Admin API

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.0% | 30-day rolling |
| **Error Budget** | 7.2 hours/month | Monthly reset |

### DevHub

#### Availability SLO

| Metric | Target | Measurement Window |
|--------|--------|-------------------|
| **Uptime** | 99.0% | 30-day rolling |
| **Error Budget** | 7.2 hours/month | Monthly reset |

---

## Error Budget Policy

### Budget Consumption Thresholds

| Consumption Level | Status | Actions Required |
|-------------------|--------|------------------|
| **0-50%** | ✅ Green | Normal operations, feature development prioritized |
| **50-75%** | 🟡 Yellow | Increased monitoring, consider pausing risky changes |
| **75-90%** | 🟠 Orange | Feature freeze, focus on stability improvements |
| **90-100%** | 🔴 Red | Full freeze, all hands on reliability |
| **>100%** | ⛔ Exhausted | Incident retrospective required, mandatory reliability sprint |

### Budget Calculation

**Monthly Error Budget (minutes)**:
```
Error_Budget = (1 - SLO_Target) × 30 × 24 × 60
```

| SLO Target | Monthly Budget (minutes) | Daily Budget (minutes) |
|------------|--------------------------|------------------------|
| 99.9% | 43.2 | 1.44 |
| 99.5% | 216 | 7.2 |
| 99.0% | 432 | 14.4 |

### Budget Consumption Events

Events that consume error budget:
- Service unavailability (5xx errors)
- Response times exceeding SLO thresholds
- Failed health checks
- Unplanned maintenance windows

Events that do NOT consume error budget:
- Planned maintenance (with proper notice)
- External dependency failures (documented)
- Client-side errors (4xx)

---

## Alert Thresholds

### Critical Alerts (Page On-Call)

| Condition | Threshold | Duration | Severity |
|-----------|-----------|----------|----------|
| Error rate | > 5% | 5 minutes | Critical |
| Availability | < 99% | 5 minutes | Critical |
| P95 latency | > 2x target | 10 minutes | Critical |
| Health check failures | 3 consecutive | Immediate | Critical |

### Warning Alerts (Notify Team)

| Condition | Threshold | Duration | Severity |
|-----------|-----------|----------|----------|
| Error rate | > 1% | 15 minutes | Warning |
| Availability | < 99.5% | 15 minutes | Warning |
| P95 latency | > 1.5x target | 15 minutes | Warning |
| Error budget consumption | > 50% monthly | Daily check | Warning |

### Informational Alerts

| Condition | Threshold | Duration | Severity |
|-----------|-----------|----------|----------|
| Traffic anomaly | > 2 std dev | 30 minutes | Info |
| Error budget consumption | > 25% monthly | Daily check | Info |
| New error patterns | Any | Continuous | Info |

---

## Monitoring Implementation

### Application Insights Queries

#### Availability SLI

```kusto
requests
| where timestamp > ago(30d)
| summarize
    TotalRequests = count(),
    FailedRequests = countif(resultCode startswith "5"),
    Availability = (count() - countif(resultCode startswith "5")) * 100.0 / count()
| project Availability, TotalRequests, FailedRequests
```

#### Latency SLI

```kusto
requests
| where timestamp > ago(30d)
| where name !contains "health" and name !contains "warmup"
| summarize
    P50 = percentile(duration, 50),
    P95 = percentile(duration, 95),
    P99 = percentile(duration, 99)
| project P50, P95, P99
```

#### Error Budget Consumption

```kusto
let sloTarget = 0.999;
let budgetMinutes = (1 - sloTarget) * 30 * 24 * 60;
requests
| where timestamp > startofmonth(now())
| summarize
    TotalRequests = count(),
    FailedRequests = countif(resultCode startswith "5"),
    DowntimeMinutes = countif(resultCode startswith "5") * 1.0 / count() * 30 * 24 * 60
| extend
    BudgetConsumed = DowntimeMinutes / budgetMinutes * 100,
    BudgetRemaining = budgetMinutes - DowntimeMinutes
| project BudgetConsumed, BudgetRemaining, DowntimeMinutes
```

### Azure Monitor Alert Rules

```json
{
  "alertRules": [
    {
      "name": "Critical - High Error Rate",
      "condition": "requests/failed > 5%",
      "window": "PT5M",
      "severity": 0,
      "action": "page-oncall"
    },
    {
      "name": "Warning - Elevated Error Rate",
      "condition": "requests/failed > 1%",
      "window": "PT15M",
      "severity": 2,
      "action": "notify-team"
    },
    {
      "name": "Critical - High Latency P95",
      "condition": "requests/duration P95 > 1000ms",
      "window": "PT10M",
      "severity": 0,
      "action": "page-oncall"
    }
  ]
}
```

---

## Dashboard Requirements

### SLO Dashboard Panels

1. **Availability Overview**
   - Current availability (last 24h, 7d, 30d)
   - Error budget consumption gauge
   - Error rate time series

2. **Latency Distribution**
   - P50, P95, P99 time series
   - Latency histogram
   - Slow endpoint breakdown

3. **Error Analysis**
   - Error type breakdown
   - Top failing endpoints
   - Error trend analysis

4. **Capacity Metrics**
   - Request rate (rps)
   - Active connections
   - Resource utilization

### SLO Status Widget

Visual indicator for each service:
- 🟢 Green: Within SLO
- 🟡 Yellow: Warning threshold
- 🔴 Red: SLO breach

---

## Review Cadence

| Review Type | Frequency | Participants | Focus |
|-------------|-----------|--------------|-------|
| Daily SLO Check | Daily | On-call engineer | Error budget status |
| Weekly Review | Weekly | Platform team | Trends, incidents |
| Monthly Review | Monthly | Engineering leads | SLO adjustments, capacity |
| Quarterly Review | Quarterly | Leadership | Strategic reliability goals |

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-22 | Platform Team | Initial SLO definitions |

---

## References

- [Google SRE Book - SLOs](https://sre.google/sre-book/service-level-objectives/)
- [Azure Monitor SLI/SLO](https://learn.microsoft.com/en-us/azure/azure-monitor/app/availability-overview)
- [ADR-0014: Polyglot Persistence Framework](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md)
