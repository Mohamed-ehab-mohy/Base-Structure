# Multi-Tenant SaaS Base Structure

This repository is a proposed base structure for a medium-sized multi-tenant SaaS project.

There was no existing codebase to refactor, so this submission provides a clean starting template plus documentation that explains how an existing project would be migrated into this structure.

## Architecture Summary

The solution follows a practical Clean Architecture approach:

- `Domain` contains the core business model and rules.
- `Application` contains use cases, DTOs, interfaces, validation, and orchestration.
- `Infrastructure` contains database access, tenant resolution services, integrations, repositories, caching, and implementation details.
- `API` contains controllers, middleware, filters, request/response handling, and dependency injection entry points.
- `tests` contains automated tests separated by layer.
- `docs` contains the required folder directory map and migration changelog.

## Multi-Tenant SaaS Direction

The structure is designed to support:

- tenant isolation through a master tenant store and tenant-aware application data access
- tenant resolution from subdomain, request header, or JWT claims
- feature gating by tenant plan
- separate infrastructure concerns for persistence, caching, billing, and integrations
- clear separation between business rules and framework/database details

## Required Submission Documents

- [Folder Directory Map](docs/folder-directory-map.md)
- [Migration Changelog](docs/migration-changelog.md)

