# Caching Strategies Guide

This guide covers caching strategies used in the Mystira platform, including the cache-aside pattern for WebAssembly (WASM) applications and Redis caching differences between Blazor and React architectures.

## Table of Contents

- [Cache-Aside Pattern for WASM](#cache-aside-pattern-for-wasm)
  - [Overview](#overview)
  - [How It Works](#how-it-works)
  - [WASM Implementation](#wasm-implementation)
  - [Benefits and Trade-offs](#benefits-and-trade-offs)
- [Redis Caching: Blazor vs React](#redis-caching-blazor-vs-react)
  - [Architecture Differences](#architecture-differences)
  - [Blazor Redis Usage](#blazor-redis-usage)
  - [React Redis Usage](#react-redis-usage)
  - [Key Differences](#key-differences)
- [Implementation Examples](#implementation-examples)
- [Best Practices](#best-practices)

---

## Cache-Aside Pattern for WASM

### Overview

The **cache-aside pattern** (also known as lazy loading) is a common caching strategy where applications check a cache first before querying slower data sources like databases. This pattern is particularly effective for WebAssembly (WASM) applications running in browser or edge environments.

For WASM, this pattern optimizes performance by loading data on-demand into fast in-memory caches such as:

- **IndexedDB** - Asynchronous, large-scale browser storage
- **localStorage** - Synchronous, smaller key-value storage
- **Custom memory structures** - In-memory caches built with Rust/C++ compiled to WASM

This approach suits WASM's stateless, high-performance nature, reducing latency for compute-heavy applications by minimizing network roundtrips.

### How It Works

The cache-aside pattern follows this flow:

#### Read Path

```
┌──────────────────────────────────────────────────────────────┐
│                     Cache-Aside Read Flow                     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Application requests data                                │
│     ▼                                                        │
│  2. Check cache                                              │
│     │                                                        │
│     ├─► Cache Hit: Return data immediately                  │
│     │   (Optimal path - low latency)                        │
│     │                                                        │
│     └─► Cache Miss:                                          │
│         ▼                                                    │
│         3. Fetch from backend (e.g., API call)              │
│         ▼                                                    │
│         4. Store in cache with TTL                           │
│         ▼                                                    │
│         5. Return data to application                        │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

#### Write Path

```
┌──────────────────────────────────────────────────────────────┐
│                    Cache-Aside Write Flow                     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Application updates data                                 │
│     ▼                                                        │
│  2. Write to data source first (database/API)               │
│     ▼                                                        │
│  3. Invalidate or update cache entry                         │
│     │                                                        │
│     ├─► Option A: Invalidate (delete cache entry)           │
│     │   - Simpler, cache will refresh on next read          │
│     │                                                        │
│     └─► Option B: Update (write-through)                    │
│         - More complex, keeps cache consistent               │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Key principles:**

- **Read**: Check cache → On miss, fetch from backend → Store in cache → Return data
- **Write**: Update data source first → Invalidate or overwrite cache entry to maintain consistency

### WASM Implementation

In WASM runtimes like Wasmtime or browser contexts, you can implement the cache-aside pattern using:

- **Rust with wasm-bindgen** for browser WASM
- **JavaScript proxies** to wrap cache logic around async fetches
- **Storage APIs** like IndexedDB or localStorage

#### Example: Browser WASM with localStorage

```javascript
// JavaScript implementation wrapping WASM calls
class CacheAsideWASM {
  constructor(ttlSeconds = 300) {
    this.ttl = ttlSeconds * 1000; // Convert to milliseconds
  }

  async getData(key) {
    // 1. Check cache first
    const cached = this.getCacheEntry(key);
    if (cached && !this.isExpired(cached)) {
      console.log(`Cache hit for ${key}`);
      return cached.data;
    }

    console.log(`Cache miss for ${key}`);
    
    // 2. Fetch from backend
    const data = await fetch(`/api/data/${key}`)
      .then(res => res.json());
    
    // 3. Store in cache with TTL
    this.setCacheEntry(key, data);
    
    // 4. Return data
    return data;
  }

  getCacheEntry(key) {
    try {
      const item = localStorage.getItem(`cache:${key}`);
      return item ? JSON.parse(item) : null;
    } catch (e) {
      console.error('Cache read error:', e);
      return null;
    }
  }

  setCacheEntry(key, data) {
    try {
      const entry = {
        data: data,
        timestamp: Date.now(),
        ttl: this.ttl
      };
      localStorage.setItem(`cache:${key}`, JSON.stringify(entry));
    } catch (e) {
      console.error('Cache write error:', e);
    }
  }

  isExpired(entry) {
    return Date.now() - entry.timestamp > entry.ttl;
  }

  invalidate(key) {
    localStorage.removeItem(`cache:${key}`);
  }
}

// Usage
const cache = new CacheAsideWASM(300); // 5 minute TTL

// Read with cache
const userData = await cache.getData('user:123');

// Write - invalidate cache after update
await fetch('/api/user/123', {
  method: 'PUT',
  body: JSON.stringify(updatedUser)
});
cache.invalidate('user:123');
```

#### Example: Rust WASM with wasm-bindgen

```rust
use wasm_bindgen::prelude::*;
use wasm_bindgen::JsCast;
use wasm_bindgen_futures::JsFuture;
use web_sys::{Request, RequestInit, RequestMode, Response, window};
use serde::{Deserialize, Serialize};

#[wasm_bindgen]
pub struct CacheManager {
    ttl_seconds: u32,
}

#[wasm_bindgen]
impl CacheManager {
    #[wasm_bindgen(constructor)]
    pub fn new(ttl_seconds: u32) -> CacheManager {
        CacheManager { ttl_seconds }
    }

    // Check localStorage cache
    fn get_from_cache(&self, key: &str) -> Option<String> {
        let window = window().unwrap();
        let storage = window.local_storage().ok()??;
        storage.get_item(&format!("cache:{}", key)).ok()?
    }

    // Store in localStorage cache
    fn set_in_cache(&self, key: &str, value: &str) {
        if let Some(window) = window() {
            if let Ok(Some(storage)) = window.local_storage() {
                let _ = storage.set_item(&format!("cache:{}", key), value);
            }
        }
    }

    // Fetch data with cache-aside pattern
    pub async fn get_data(&self, key: String) -> Result<JsValue, JsValue> {
        // 1. Check cache
        if let Some(cached) = self.get_from_cache(&key) {
            return Ok(JsValue::from_str(&cached));
        }

        // 2. Cache miss - fetch from API
        let mut opts = RequestInit::new();
        opts.method("GET");
        opts.mode(RequestMode::Cors);

        let url = format!("/api/data/{}", key);
        let request = Request::new_with_str_and_init(&url, &opts)?;

        let window = window().unwrap();
        let resp_value = JsFuture::from(window.fetch_with_request(&request)).await?;
        let resp: Response = resp_value.dyn_into().unwrap();

        // 3. Get response text
        let text = JsFuture::from(resp.text()?).await?;
        let data = text.as_string().unwrap();

        // 4. Store in cache
        self.set_in_cache(&key, &data);

        // 5. Return data
        Ok(JsValue::from_str(&data))
    }

    pub fn invalidate(&self, key: String) {
        if let Some(window) = window() {
            if let Ok(Some(storage)) = window.local_storage() {
                let _ = storage.remove_item(&format!("cache:{}", key));
            }
        }
    }
}
```

#### Example: Using IndexedDB for Larger Datasets

For larger datasets, IndexedDB provides better performance than localStorage:

```javascript
import { get, set, del } from 'idb-keyval';

class IndexedDBCache {
  constructor(ttlSeconds = 300) {
    this.ttl = ttlSeconds * 1000;
  }

  async getData(key) {
    // 1. Check IndexedDB cache
    const cached = await get(key);
    
    if (cached && !this.isExpired(cached)) {
      console.log(`Cache hit for ${key}`);
      return cached.data;
    }

    console.log(`Cache miss for ${key}`);
    
    // 2. Fetch from backend
    const data = await fetch(`/api/data/${key}`)
      .then(res => res.json());
    
    // 3. Store in IndexedDB with timestamp
    await set(key, {
      data: data,
      timestamp: Date.now(),
      ttl: this.ttl
    });
    
    return data;
  }

  isExpired(entry) {
    return Date.now() - entry.timestamp > entry.ttl;
  }

  async invalidate(key) {
    await del(key);
  }
}
```

### Benefits and Trade-offs

#### Pros

- **Scales with unpredictable loads**: Cache automatically grows with demand
- **Minimizes cold starts**: Especially important in serverless WASM environments
- **Boosts throughput**: Offloads read operations from backend
- **Reduces latency**: In-memory or local storage access is much faster than network calls
- **Simple to implement**: Straightforward logic with clear separation
- **Works offline**: Cached data available even without network

#### Cons

- **Stale data risk**: If cache invalidation fails, users see outdated information
- **Added complexity**: Application must manage cache lifecycle and TTLs
- **Cache warming**: First request is slow (cold start)
- **Memory constraints**: Browser storage has limits (localStorage: ~5-10MB, IndexedDB: larger but browser-dependent)
- **Consistency challenges**: Keeping cache synchronized with source of truth

#### When to Use Cache-Aside for WASM

✅ **Good fits:**
- Read-heavy workloads (90%+ reads)
- Data that tolerates slight staleness (user preferences, configuration)
- Compute-intensive WASM operations that need fast data access
- Static or slowly-changing content (scenarios, game rules)
- Offline-first applications

❌ **Poor fits:**
- Real-time data requiring strong consistency
- Frequently updated data
- Highly personalized data that can't be shared
- Sensitive data that shouldn't be cached client-side

---

## Redis Caching: Blazor vs React

Cache strategies for Redis differ significantly between Blazor (server-centric .NET) and React (client-centric JavaScript) due to their fundamental architectural differences.

### Architecture Differences

```
┌─────────────────────────────────────────────────────────────────┐
│               Blazor Server vs React Architecture                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  BLAZOR SERVER (Server-Centric)                                 │
│  ┌──────────────┐                                               │
│  │   Browser    │                                               │
│  │  (UI only)   │                                               │
│  └──────┬───────┘                                               │
│         │ SignalR                                               │
│         ▼                                                       │
│  ┌──────────────────────────────────────┐                      │
│  │        ASP.NET Core Server           │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │    Blazor Circuit/Session      │  │                      │
│  │  │  - Component state             │  │                      │
│  │  │  - Event handlers              │  │                      │
│  │  └────────────────────────────────┘  │                      │
│  │                │                      │                      │
│  │                ▼                      │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │       Redis Cache              │  │                      │
│  │  │  - IDistributedCache           │  │                      │
│  │  │  - Shared across instances     │  │                      │
│  │  │  - Session state               │  │                      │
│  │  └────────────────────────────────┘  │                      │
│  └──────────────────────────────────────┘                      │
│                                                                 │
│  REACT (Client-Centric)                                         │
│  ┌──────────────────────────────────────┐                      │
│  │           Browser                    │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │     React Application          │  │                      │
│  │  │  - Component state (local)     │  │                      │
│  │  │  - Client-side cache           │  │                      │
│  │  │    (Redux, React Query)        │  │                      │
│  │  └────────────┬───────────────────┘  │                      │
│  └───────────────┼──────────────────────┘                      │
│                  │ HTTP/REST                                   │
│                  ▼                                              │
│  ┌──────────────────────────────────────┐                      │
│  │      Node.js/Express Backend         │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │       Redis Cache              │  │                      │
│  │  │  - API response cache          │  │                      │
│  │  │  - Session store               │  │                      │
│  │  │  - Real-time data              │  │                      │
│  │  └────────────────────────────────┘  │                      │
│  └──────────────────────────────────────┘                      │
│                                                                 │
│  BLAZOR WASM (Hybrid)                                           │
│  ┌──────────────────────────────────────┐                      │
│  │           Browser                    │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │     Blazor WASM App            │  │                      │
│  │  │  - IndexedDB                   │  │                      │
│  │  │  - localStorage                │  │                      │
│  │  │  - In-memory cache             │  │                      │
│  │  └────────────┬───────────────────┘  │                      │
│  └───────────────┼──────────────────────┘                      │
│                  │ HTTP/REST                                   │
│                  ▼                                              │
│  ┌──────────────────────────────────────┐                      │
│  │      ASP.NET Core Web API            │                      │
│  │  ┌────────────────────────────────┐  │                      │
│  │  │       Redis Cache              │  │                      │
│  │  │  - API response cache          │  │                      │
│  │  └────────────────────────────────┘  │                      │
│  └──────────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘
```

### Blazor Redis Usage

#### Blazor Server

Blazor Server uses **server-side Redis** for shared state across users and server instances. The application runs on the server, with UI updates sent to the browser via SignalR.

**Key characteristics:**

- **Shared state**: Multiple server instances share the same Redis cache
- **Circuit state**: Store Blazor circuit/session state in Redis
- **Heavy datasets**: Cache large data structures that would be expensive to transfer to client
- **Pub/Sub**: Use Redis Pub/Sub for cache invalidation across instances

**Implementation example:**

```csharp
// Startup.cs or Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Redis distributed cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = "Mystira:";
        });

        // Hybrid approach: MemoryCache + Redis
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IHybridCache, HybridCache>();

        var app = builder.Build();
        app.Run();
    }
}

// HybridCache.cs - Layer MemoryCache before Redis
public class HybridCache : IHybridCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCache> _logger;
    
    public HybridCache(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCache> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        // 1. Check local memory cache (fast)
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("MemoryCache hit for {Key}", key);
            return cachedValue;
        }

        // 2. Check Redis distributed cache
        var redisValue = await _distributedCache.GetStringAsync(key, ct);
        if (redisValue != null)
        {
            _logger.LogDebug("Redis hit for {Key}", key);
            var value = JsonSerializer.Deserialize<T>(redisValue);
            
            // 3. Populate memory cache
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
            return value;
        }

        _logger.LogDebug("Cache miss for {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan expiration,
        CancellationToken ct = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        
        // Write to both caches
        _memoryCache.Set(key, value, expiration);
        
        await _distributedCache.SetStringAsync(
            key, 
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            },
            ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key, ct);
    }
}

// ScenarioService.cs - Using the cache
public class ScenarioService
{
    private readonly IHybridCache _cache;
    private readonly IScenarioRepository _repository;

    public ScenarioService(IHybridCache cache, IScenarioRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Scenario?> GetScenarioAsync(string id)
    {
        var cacheKey = $"scenario:{id}";
        
        // Try cache first
        var cached = await _cache.GetAsync<Scenario>(cacheKey);
        if (cached != null)
            return cached;

        // Load from database
        var scenario = await _repository.GetByIdAsync(id);
        if (scenario != null)
        {
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, scenario, TimeSpan.FromHours(1));
        }

        return scenario;
    }

    public async Task UpdateScenarioAsync(Scenario scenario)
    {
        await _repository.UpdateAsync(scenario);
        
        // Invalidate cache
        var cacheKey = $"scenario:{scenario.Id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

**Redis Pub/Sub for invalidation across instances:**

```csharp
// CacheInvalidationService.cs
public class CacheInvalidationService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHybridCache _cache;
    private readonly ILogger<CacheInvalidationService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        
        await subscriber.SubscribeAsync("cache:invalidate", (channel, message) =>
        {
            _logger.LogInformation("Received invalidation for {Key}", message);
            _cache.RemoveAsync(message).GetAwaiter().GetResult();
        });
    }
}

// Publishing invalidation
public async Task InvalidateCacheGloballyAsync(string key)
{
    var subscriber = _redis.GetSubscriber();
    await subscriber.PublishAsync("cache:invalidate", key);
}
```

#### Blazor WebAssembly (WASM)

Blazor WASM favors **client-side caches** like IndexedDB, using Redis indirectly via APIs.

```csharp
// ClientCacheService.cs (runs in browser)
public class ClientCacheService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public async Task<T?> GetDataAsync<T>(string key)
    {
        // Check IndexedDB via JS interop
        var cached = await _js.InvokeAsync<string>("localStorageGet", $"cache:{key}");
        
        if (cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached);
        }

        // Fetch from API (which may use Redis server-side)
        var data = await _http.GetFromJsonAsync<T>($"/api/data/{key}");
        
        if (data != null)
        {
            var serialized = JsonSerializer.Serialize(data);
            await _js.InvokeVoidAsync("localStorageSet", $"cache:{key}", serialized);
        }

        return data;
    }
}
```

### React Redis Usage

React applications cache Redis data **client-side** after API fetches, using libraries like Redux, React Query, or custom hooks.

**Key characteristics:**

- **Client-side caching**: State management libraries cache API responses
- **URL-based keys**: Often hash request URLs to create cache keys
- **Short TTLs**: Typically 1-5 minutes to balance freshness and performance
- **Optimistic updates**: Update UI immediately, sync with backend asynchronously
- **Stale-while-revalidate**: Show cached data while fetching fresh data

**Implementation example with React Query:**

```typescript
// hooks/useScenario.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

interface Scenario {
  id: string;
  title: string;
  content: string;
}

// Fetch scenario (React Query caches automatically)
export function useScenario(id: string) {
  return useQuery({
    queryKey: ['scenario', id],
    queryFn: async () => {
      const response = await fetch(`/api/scenarios/${id}`);
      if (!response.ok) throw new Error('Failed to fetch scenario');
      return response.json() as Promise<Scenario>;
    },
    staleTime: 2 * 60 * 1000, // Consider fresh for 2 minutes
    cacheTime: 5 * 60 * 1000, // Keep in cache for 5 minutes
  });
}

// Update scenario with cache invalidation
export function useUpdateScenario() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (scenario: Scenario) => {
      const response = await fetch(`/api/scenarios/${scenario.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(scenario),
      });
      if (!response.ok) throw new Error('Failed to update scenario');
      return response.json();
    },
    onSuccess: (data, variables) => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: ['scenario', variables.id] });
      
      // Or optimistically update
      queryClient.setQueryData(['scenario', variables.id], data);
    },
  });
}

// Component usage
function ScenarioEditor({ scenarioId }: { scenarioId: string }) {
  const { data: scenario, isLoading, error } = useScenario(scenarioId);
  const updateMutation = useUpdateScenario();

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  const handleSave = () => {
    updateMutation.mutate(scenario);
  };

  return (
    <div>
      <h1>{scenario.title}</h1>
      {/* Editor UI */}
      <button onClick={handleSave}>Save</button>
    </div>
  );
}
```

**Server-side Redis caching (Node.js/Express):**

```typescript
// server/middleware/redisCache.ts
import { createClient } from 'redis';
import { Request, Response, NextFunction } from 'express';

const redisClient = createClient({
  url: process.env.REDIS_URL,
});

await redisClient.connect();

// Cache middleware
export function cacheMiddleware(durationSeconds: number) {
  return async (req: Request, res: Response, next: NextFunction) => {
    // Hash URL to create cache key
    const key = `cache:${req.method}:${req.originalUrl}`;

    try {
      // Check Redis cache
      const cachedResponse = await redisClient.get(key);
      
      if (cachedResponse) {
        console.log(`Cache hit: ${key}`);
        return res.json(JSON.parse(cachedResponse));
      }

      console.log(`Cache miss: ${key}`);

      // Store original res.json
      const originalJson = res.json.bind(res);

      // Override res.json to cache response
      res.json = (body: any) => {
        // Cache the response
        redisClient.setEx(key, durationSeconds, JSON.stringify(body));
        return originalJson(body);
      };

      next();
    } catch (error) {
      console.error('Redis cache error:', error);
      next(); // Continue without cache on error
    }
  };
}

// routes/scenarios.ts
import express from 'express';
import { cacheMiddleware } from '../middleware/redisCache';

const router = express.Router();

// Cache scenario for 5 minutes
router.get('/scenarios/:id', cacheMiddleware(300), async (req, res) => {
  const scenario = await db.scenarios.findById(req.params.id);
  res.json(scenario);
});

// Invalidate cache on update
router.put('/scenarios/:id', async (req, res) => {
  const updated = await db.scenarios.update(req.params.id, req.body);
  
  // Invalidate cache
  await redisClient.del(`cache:GET:/api/scenarios/${req.params.id}`);
  
  res.json(updated);
});
```

**Stale-while-revalidate pattern:**

```typescript
// hooks/useStaleWhileRevalidate.ts
import { useState, useEffect } from 'react';

export function useStaleWhileRevalidate<T>(
  key: string,
  fetcher: () => Promise<T>,
  ttlMs: number = 60000
) {
  const [data, setData] = useState<T | null>(null);
  const [isValidating, setIsValidating] = useState(false);

  useEffect(() => {
    let cancelled = false;

    const fetchData = async () => {
      // 1. Check localStorage cache
      const cached = localStorage.getItem(key);
      const cachedData = cached ? JSON.parse(cached) : null;

      if (cachedData && Date.now() - cachedData.timestamp < ttlMs) {
        // Show stale data immediately
        setData(cachedData.data);
      }

      // 2. Fetch fresh data in background
      setIsValidating(true);
      try {
        const fresh = await fetcher();
        if (!cancelled) {
          setData(fresh);
          localStorage.setItem(key, JSON.stringify({
            data: fresh,
            timestamp: Date.now()
          }));
        }
      } catch (error) {
        console.error('Revalidation error:', error);
      } finally {
        if (!cancelled) {
          setIsValidating(false);
        }
      }
    };

    fetchData();

    return () => {
      cancelled = true;
    };
  }, [key, ttlMs]);

  return { data, isValidating };
}
```

### Key Differences

| Aspect | Blazor Server | Blazor WASM | React (Client) |
|--------|---------------|-------------|----------------|
| **Cache Location** | Server-side shared Redis | Client-side (IndexedDB/localStorage) | Client-side + API proxy Redis |
| **Primary Libraries** | `IDistributedCache`, `MemoryCache` | JS interop to browser storage | React Query, Redux, SWR |
| **Invalidation** | Pub/Sub across server instances | Local invalidation only | Hooks, API polling, WebSockets |
| **Scalability** | Multi-server consistency via Redis | Per-user, no server state | Per-user, reduces API load |
| **TTL Strategy** | Long TTLs (hours) for static content | Medium TTLs (minutes to hours) | Short TTLs (1-5 minutes) |
| **Consistency** | Strong (single source) | Eventual (client may be stale) | Eventual (optimistic updates) |
| **Network Access** | Direct Redis connection | Via HTTP APIs only | Via HTTP APIs only |
| **Best For** | Heavy server-side workloads | Offline-first apps | High interactivity, frequent updates |

---

## Implementation Examples

### Mystira Platform: Scenario Caching

For the Mystira platform, here's how we can implement caching for scenarios across different components:

#### Admin API (C#/.NET)

```csharp
// Services/CachedScenarioService.cs
public class CachedScenarioService : IScenarioService
{
    private readonly IScenarioRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedScenarioService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<Scenario?> GetScenarioAsync(string id, CancellationToken ct = default)
    {
        var cacheKey = $"mystira:scenario:{id}";

        // Check cache
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for scenario {Id}", id);
            return JsonSerializer.Deserialize<Scenario>(cached);
        }

        _logger.LogDebug("Cache miss for scenario {Id}", id);

        // Load from database
        var scenario = await _repository.GetByIdAsync(id, ct);
        if (scenario != null)
        {
            // Cache the scenario
            var serialized = JsonSerializer.Serialize(scenario);
            await _cache.SetStringAsync(
                cacheKey,
                serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                },
                ct);
        }

        return scenario;
    }

    public async Task UpdateScenarioAsync(Scenario scenario, CancellationToken ct = default)
    {
        await _repository.UpdateAsync(scenario, ct);

        // Invalidate cache
        var cacheKey = $"mystira:scenario:{scenario.Id}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        _logger.LogInformation("Invalidated cache for scenario {Id}", scenario.Id);
    }
}
```

#### Publisher Service (TypeScript/Node.js)

```typescript
// services/scenarioCache.ts
import { createClient } from 'redis';

export class ScenarioCacheService {
  private redis = createClient({ url: process.env.REDIS_URL });
  private readonly TTL = 3600; // 1 hour

  async connect() {
    await this.redis.connect();
  }

  async getScenario(id: string): Promise<Scenario | null> {
    const key = `mystira:scenario:${id}`;
    
    try {
      // Check Redis
      const cached = await this.redis.get(key);
      if (cached) {
        console.log(`Cache hit for scenario ${id}`);
        return JSON.parse(cached);
      }

      console.log(`Cache miss for scenario ${id}`);

      // Fetch from Admin API
      const response = await fetch(`${process.env.ADMIN_API_URL}/api/scenarios/${id}`);
      if (!response.ok) return null;

      const scenario = await response.json();

      // Cache the result
      await this.redis.setEx(key, this.TTL, JSON.stringify(scenario));

      return scenario;
    } catch (error) {
      console.error('Cache error:', error);
      return null;
    }
  }

  async invalidateScenario(id: string): Promise<void> {
    const key = `mystira:scenario:${id}`;
    await this.redis.del(key);
    console.log(`Invalidated cache for scenario ${id}`);
  }
}
```

#### Admin UI (React/TypeScript)

```typescript
// hooks/useScenario.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

export function useScenario(id: string) {
  return useQuery({
    queryKey: ['scenario', id],
    queryFn: async () => {
      const response = await fetch(`/admin/api/scenarios/${id}`);
      if (!response.ok) throw new Error('Failed to fetch');
      return response.json();
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    cacheTime: 10 * 60 * 1000, // 10 minutes
  });
}

export function useUpdateScenario() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (scenario: Scenario) => {
      const response = await fetch(`/admin/api/scenarios/${scenario.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(scenario),
      });
      return response.json();
    },
    onSuccess: (_, variables) => {
      // Invalidate React Query cache
      queryClient.invalidateQueries({ queryKey: ['scenario', variables.id] });
    },
  });
}
```

---

## Best Practices

### General Caching Principles

1. **Set appropriate TTLs**
   - Static content: Hours to days
   - Dynamic content: Minutes to hours
   - Real-time data: Seconds or no cache

2. **Use cache keys strategically**
   - Include version/namespace: `mystira:v1:scenario:123`
   - Use consistent naming: `{app}:{type}:{id}`
   - Consider user context: `user:{userId}:preferences`

3. **Handle cache failures gracefully**
   - Always have a fallback to source data
   - Log cache errors but don't fail requests
   - Monitor cache hit rates

4. **Invalidate proactively**
   - Clear cache on updates
   - Use pub/sub for distributed invalidation
   - Batch invalidations when possible

### WASM-Specific

5. **Respect browser storage limits**
   - localStorage: ~5-10MB
   - IndexedDB: Much larger, but browser-dependent
   - Implement cache eviction policies (LRU)

6. **Use appropriate storage**
   - Small, simple data → localStorage
   - Large datasets → IndexedDB
   - Temporary data → Memory cache

7. **Optimize for offline**
   - Cache critical user data
   - Implement background sync
   - Show clear indicators when offline

### Redis-Specific

8. **Layer caches for Blazor Server**
   - L1: MemoryCache (5-30 second TTL)
   - L2: Redis (minutes to hours)
   - L3: Database

9. **Use Redis data structures**
   - Strings for simple values
   - Hashes for objects
   - Sets for collections
   - Sorted sets for ranked data

10. **Monitor Redis memory**
    - Set max memory limits
    - Configure eviction policies (allkeys-lru)
    - Monitor cache hit rates

### Security

11. **Don't cache sensitive data client-side**
    - Authentication tokens → Memory only (not localStorage)
    - Personal information → Server-side cache only
    - Payment data → Never cache

12. **Validate cached data**
    - Check data integrity
    - Implement cache versioning
    - Handle corrupted cache gracefully

### Performance

13. **Compress large payloads**
    - Use gzip/brotli for cached responses
    - Consider MessagePack for binary data

14. **Batch cache operations**
    - Use `mget`/`mset` for multiple keys
    - Pipeline Redis commands
    - Reduce network roundtrips

15. **Monitor and measure**
    - Track cache hit/miss rates
    - Measure response times
    - Alert on cache failures

---

## Related Documentation

- [ADR-0013: Data Management and Storage Strategy](../architecture/adr/0013-data-management-and-storage-strategy.md)
- [ADR-0014: Polyglot Persistence Framework Selection](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [Infrastructure: Shared Resources](../infrastructure/shared-resources.md)

---

## References

- [Cache-Aside Pattern (Microsoft)](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Redis Best Practices](https://redis.io/docs/manual/client-side-caching/)
- [React Query Documentation](https://tanstack.com/query/latest/docs/react/overview)
- [ASP.NET Core Distributed Caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [WebAssembly with Rust](https://rustwasm.github.io/docs/book/)
