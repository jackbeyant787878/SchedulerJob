# SchedulerJobCenter Distributed Scheduled Task Scheduling Center

**✨ Production\-Grade Distributed Task Scheduling Platform Built on \.NET 10 \+ Quartz\.NET \+ CQRS Architecture**

SchedulerJobCenter eliminates the long\-standing drawbacks of scattered, self\-hosted scheduled tasks within individual microservices\. It provides a centralized scheduling hub to uniformly manage all periodic jobs across the entire cluster\. It features **custom time zone scheduling, dynamic task configuration, high\-concurrency fault tolerance, automatic retry logic, full\-link logging and monitoring, and scheduled log cleanup**\. Lightweight and highly available, it supports out\-of\-the\-box cluster deployment and perfectly integrates with modern \.NET microservice architectures\.

## 📖 Project Background

In traditional microservice development, most teams embed independent scheduling frameworks into every business service\. This fragmented approach introduces widespread architectural and operational pain points:

- **Severe Service Coupling**: Scheduling logic pollutes business services, resulting in bloated service code, blurred single responsibilities, and substantially increased maintenance overhead\.

- **Decentralized Task Management**: Tasks run independently across multiple services with no unified dashboard or governance mechanism, making batch administration, auditing, and fault troubleshooting extremely difficult\.

- **Time Zone \& Scheduling Inaccuracy**: Most open\-source schedulers lack precise time zone awareness\. Cross\-region deployment frequently causes task drift, missed executions, and duplicate triggers due to UTC conversion bias and daylight saving time changes\.

- **Poor High\-Concurrency Fault Tolerance**: Native support for concurrency control, idempotency validation, and failure retry is missing, leading to unstable task execution in high\-load production environments\.

- **Heavy Operational Overhead**: Traditional scheduling systems require service restarts for any task modification, lacking dynamic configuration capabilities and real\-time operational flexibility\.

Built to resolve the above limitations, SchedulerJobCenter delivers a**centralized, decoupled, observable, and highly available** unified scheduling solution for microservice ecosystems\. Business services no longer need to integrate any scheduling components — only public HTTP endpoints are required to receive scheduled job triggers\.

## ⚙️ Core Technology Selection \& Comparison

Replacing the commonly used Hangfire framework, this project adopts the **latest stable Quartz\.NET** as its core scheduling engine\. The selection prioritizes production\-critical capabilities including precise time zone scheduling, second\-level cron accuracy, dynamic task manipulation, and stable cluster HA\. A detailed feature comparison is shown below:

|Comparison Dimension|Hangfire|Quartz\.NET \(Project Selection\)|
|---|---|---|
|Cron Expression Accuracy|Only 5\-digit minute\-level precision; second\-level scheduling requires commercial licensing|Native 6\-digit standard Cron support \(second\-level baseline\), with millisecond\-level execution precision|
|Time Zone Scheduling|Limited time zone support; UTC\-based storage frequently causes cluster time offset errors and daylight saving time anomalies|First\-class TimeZoneInfo integration with full IANA time zone support; tasks strictly follow user\-specified time zones|
|Concurrency Control|Global service\-level locking only, without fine\-grained per\-task concurrency isolation|Native `DisallowConcurrentExecution` attribute ensures single\-task isolation and inherent execution idempotency|
|Cluster High Availability|Intense lock contention in free edition; full cluster capabilities are commercially gated|Built\-in AdoJobStore distributed clustering; fully open\-source, lock\-free, and production\-stable|
|Dynamic Task Adaptation|Relies on runtime method reflection; does not support dynamic HTTP endpoint configuration with poor flexibility|Fully dynamic task creation, editing, suspension and resumption without service restarts|
|Open Source License|LGPL \(introduces potential commercial usage compliance risks\)|Apache 2\.0 \(permissive, unrestricted commercial and open\-source usage\)|

### Technology Stack Overview

- **Core Framework**: \.NET 10 \(high\-performance cross\-platform runtime\), Quartz\.NET \(industrial\-grade scheduling kernel\)

- **Architecture Pattern**: DDD \(Domain\-Driven Design\), CQRS Read\-Write Separation, Frontend\-Backend Separation

- **Data Persistence**: EF Core 9\.0, SQL Server, automatic database migration

- **Core Components**: MediatR \(CQRS pipeline scheduling\), Serilog \(structured full\-link logging\)

- **Deployment**: Docker containerization, Docker Compose one\-click orchestration, cross\-platform Windows/Linux support

- **Compatibility**: Fully compatible with \.NET 6 / \.NET 7 / \.NET 8 / \.NET 10 microservice systems

## 🏗️ Overall Technical Architecture

The project adopts a **standard five\-layer hierarchical architecture combined with CQRS read\-write separation** to achieve complete decoupling and clear responsibility division\. The API layer is ultra\-lightweight with all complex business logic sunk into dedicated handlers, conforming to enterprise\-grade scalable and maintainable design standards while eliminating messy directory and layer confusion\.

### Request Execution Pipeline

Frontend Request → Controller \(Pure Routing Forwarding\) → MediatR \(Command/Query Dispatching\) → Handler \(Core Business Logic\) → Repository \(Data Access\) → Quartz Scheduler / Database

### Standardized Project Structure

All modules are uniformly organized under the solution root with a clean, Git\-friendly structure:

```plain text
SchedulerJobCenter_Root/
├── SchedulerJobCenter.sln                 # Solution entry
├── SchedulerJobCenter.Api/                # Startup & API presentation layer
│   ├── Controllers/                       # Minimal routing-only controllers
│   ├── Filters/                           # Global exception filters
│   ├── Middleware/                        # Request logging middleware
│   ├── appsettings.json                   # Global configuration file
│   └── Program.cs                         # Application startup entry
├── SchedulerJobCenter.Application/        # CQRS business application layer
│   ├── Commands/                          # Write operations (Create/Update/Delete/Start/Stop)
│   ├── Queries/                           # Read operations (List/Detail/Validation)
│   ├── DTOs/                              # Data transfer objects
│   ├── Validators/                        # Request parameter validation
│   └── Extensions/                        # Service registration extensions
├── SchedulerJobCenter.Domain/             # Core domain layer
│   ├── Entities/                          # Business domain entities
│   └── Enums/                             # Global enumeration definitions
├── SchedulerJobCenter.Infrastructure/     # Infrastructure implementation layer
│   ├── Data/                              # EF Core context & migration records
│   ├── Repositories/                      # Data persistence repositories
│   ├── Scheduling/                        # Quartz scheduling core implementation
│   ├── BackgroundServices/                # Background long-running services
│   ├── Configuration/                     # Strongly typed configuration models
│   └── Extensions/                        # Infrastructure module registration
└── SchedulerJobCenter.Shared/             # Common shared infrastructure
    ├── Unified API response models
    └── General pagination utility models
```

### Layer Responsibilities

- **API Layer**: Handles route matching, basic parameter verification, and unified response wrapping; contains zero business logic\.

- **Application Layer \(CQRS\)**: Encapsulates all business workflows, strictly separating write commands and read queries for independent iteration and unit testing\.

- **Domain Layer**: Defines core business entities, rules and enumerations with zero external framework dependencies\.

- **Infrastructure Layer**: Implements data persistence, task scheduling, background cleaning, HTTP client management and external capabilities\.

- **Shared Layer**: Provides globally shared models, utilities and base types for cross\-project reference\.

## 🔥 Core Advantages \& Technical Highlights

### 1\. Fully Decoupled \& Standardized Architecture

- **Business\-Scheduling Isolation**: Microservices completely free from scheduling component integration, focusing solely on core business logic and eliminating service bloating\.

- **Pure CQRS Implementation**: No business logic resides in controllers; all logic is centralized in handlers, ensuring clean code structure, high maintainability and testability\.

- **Strict Hierarchical Isolation**: Adheres to DDD and dependency inversion principles with independent domain, business, infrastructure and presentation layers\.

### 2\. Accurate Time Zone\-Aware Scheduling

- Decouples task triggering from server local time, supporting full IANA standard time zone configuration \(Asia/Shanghai, UTC, Europe/America time zones, etc\.\)\.

- Tasks execute strictly based on **user\-configured time zones** rather than server time, eliminating execution offset, missed tasks and duplicate runs caused by cross\-region deployment and daylight saving time switching\.

- Frontend provides time zone selection, real\-time Cron validation, and preview of the next 5 scheduled execution times for intuitive configuration\.

### 3\. Production\-Grade Concurrency Control \& High Availability

- **Built\-In Concurrency Prevention**: Leverages Quartz native constraints to prohibit parallel execution of the same task, guaranteeing execution idempotency\.

- **Adaptive Retry Mechanism**: Supports custom retry counts and exponential backoff strategies to resolve transient failures caused by network jitter or service spikes\.

- **Task Timeout Circuit Breaking**: Independent timeout thresholds per task prevent blocking and scheduling queue accumulation\.

- **Distributed Cluster HA**: Supports multi\-node cluster deployment and automatic failover; single\-node failures do not affect global task scheduling stability\.

### 4\. Zero\-Downtime Dynamic Operation \& Maintenance

- Fully dynamic task management: supports real\-time creation, editing, suspension, resumption and deletion of jobs, including instant modification of Cron expressions, request URLs, headers, bodies, time zones and timeout settings\.

- Supports manual one\-shot task triggering to meet temporary execution demands without waiting for scheduled cycles\.

- All configuration changes take effect instantly without service restarts or business interruption\.

### 5\. Full\-Link Observability \& Automated Governance

- Records comprehensive task execution metrics: execution status, latency, HTTP status codes, response content, exception stack traces and retry histories for precise fault diagnosis\.

- **Automatic Log Cleanup**: Configurable log retention policies with background scheduled cleaning to prevent unlimited database growth\.

- Global unified exception handling and structured logging enable second\-level online problem localization\.

### 6\. Lightweight, Highly Adaptive \& Easy to Deploy

- Supports HTTP GET/POST requests with customizable headers and payloads, adapting to all standard microservice invocation scenarios\.

- Optimized HttpClient connection pooling ensures stable invocation under high\-concurrency pressure\.

- Supports Docker one\-click deployment and Docker Compose orchestration, compatible with Windows and Linux environments\.

- Built\-in automatic database migration eliminates manual table structure initialization, enabling out\-of\-the\-box usage\.

## 📋 Core Feature List

- ✅ 6\-digit second\-level Cron scheduling with real\-time validation and execution preview

- ✅ IANA standard time zone scheduling for cross\-region precise task triggering

- ✅ Full task lifecycle management \(CRUD, suspend, resume, manual trigger\)

- ✅ Automatic failure retry, timeout circuit breaking and concurrency interception

- ✅ Dynamic HTTP GET/POST invocation with custom headers, bodies and timeout control

- ✅ Full\-link execution logging, paginated query and historical traceability

- ✅ Scheduled automatic expired log cleaning for database lightweight governance

- ✅ Global unified exception handling and request monitoring

- ✅ Built\-in Swagger API documentation, health check and CORS support

- ✅ Containerized deployment and distributed cluster high availability support

## 🚀 Quick Deployment

### Environment Prerequisites

- \.NET 10 SDK / Runtime

- SQL Server 2019 or later

- Docker \(optional, for containerized deployment\)

### Deployment Methods

1. **Source Code Deployment**: Clone repository → Restore NuGet packages → Configure database connection → Build \& Run \(auto database migration\)

2. **Docker Deployment**: Execute docker\-compose\.yml to launch API and SQL Server containers in one click

## 📌 Project Summary

SchedulerJobCenter is a **production\-grade distributed task scheduling platform exclusively optimized for \.NET microservice ecosystems**\. It avoids the commercial limitations and functional deficiencies of Hangfire, adopting native Quartz\.NET scheduling capabilities and standardized CQRS layered architecture\. It fundamentally solves common microservice scheduling pain points including decentralized task management, inaccurate time zone execution, concurrency anomalies and cumbersome operation maintenance\.

With standardized architecture, thorough code decoupling, robust fault tolerance and comprehensive observability, the project maintains lightweight characteristics and low deployment costs\. It supports both standalone and cluster deployment scenarios and can be directly deployed to production for medium and large\-scale distributed systems, serving as a high\-cost\-performance, enterprise\-level unified scheduling solution for the \.NET ecosystem\.


