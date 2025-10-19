<!--
Sync Impact Report
- Version change: N/A → 1.0.0
- Modified principles: N/A (initial ratification)
- Added sections: Engineering Constraints & Standards; Development Workflow & Quality Gates
- Removed sections: None
- Templates requiring updates:
	✅ .specify/templates/plan-template.md
	✅ .specify/templates/spec-template.md
	✅ .specify/templates/tasks-template.md
	⚠ Pending: .specify/templates/commands/* (no command templates found; verify when added)
- Follow-up TODOs: None
-->

# Persistent Heap Constitution

## Core Principles

### I. Correctness First via Property-Based Testing (NON-NEGOTIABLE)

- All new behaviors MUST be specified as properties and tested with property-based tests.
- Example and unit tests MAY exist, but property-based tests are the primary safety net.
- Bug fixes MUST add a failing property that reproduces the defect before the fix.
- Randomized generators MUST include shrinking so minimal counterexamples are reported.
- Properties MUST assert B+Tree invariants (ordering, partitioning, determinism) where relevant.

Rationale: The data structure admits a vast input space; properties provide broader coverage and
catch subtle invariant violations beyond curated examples.

### II. Performance as a Contract (after correctness)

- Performance regressions MUST be measured. Benchmark harnesses (BenchmarkDotNet) are authoritative.
- PRs with potential perf impact MUST include an updated benchmark comparison vs. the latest baseline.
- Default tolerance: up to 5% degradation allowed without justification; anything beyond MUST include
either a mitigation or a clear rationale and a follow-up task to recover.
- Algorithmic guarantees (e.g., O(log n) ops) MUST be preserved; hot paths SHOULD minimize allocations.

Rationale: The project targets high-performance persistent data structures; after correctness,
throughput and latency are the key quality attributes.

### III. B+Tree Invariants are Enforced, Observable, and Testable

- Leaf keys MUST be strictly ascending; internal key ranges MUST be disjoint and correctly partition
	child subtrees.
- Search path selection MUST follow: key < K[i] → P[i]; key == K[i] → P[i+1]; else scan right; rightmost
	child selected when key exceeds all K.
- Splits MUST preserve order and parent/child relationships; leaf splits COPY UP the separator, internal
	splits MOVE UP the separator (as implemented).
- The linked list of leaves MUST iterate all keys in-order.
- Public operations MUST be deterministic for a given input sequence.

Rationale: These invariants define the structure’s correctness and are already reflected in tests and
implementation; the constitution makes them non-negotiable contracts.

### IV. API Stability and Simplicity

- Public surface SHOULD remain minimal and generic: BPlusTree<TKey, TVal> with TKey : IComparable<TKey>.
- Breaking API changes MUST be justified with migration notes and scheduled for a major release of the
	library.
- Unsafe or low-level optimizations are permitted but MUST be documented and covered by properties.

Rationale: Focus on a clear, composable core that can be optimized without churn for users.

### V. Observability and Reproducibility

- Development-time hooks (e.g., NodeSplitting/NodeSplit events) SHOULD be used for invariant validation
	in tests and debugging output.
- Property-based tests MUST support deterministic reproduction (record seeds/counterexamples in failures).
- Benchmarks MUST capture environment and parameters in output; significant differences MUST be called out
	in PRs.

Rationale: Fast diagnosis and repeatability reduce MTTR and improve developer velocity.

## Engineering Constraints & Standards

- Language/Runtime: .NET (targeting net9.0+ as per project files). Nullable enabled. Unsafe allowed.
- Testing: xUnit (facts) + property-based testing (e.g., FsCheck) + FluentAssertions for clarity.
- Benchmarking: BenchmarkDotNet; store results alongside code and reference deltas in PRs.
- Data structure invariants (non-exhaustive) to encode as properties and/or assertions:
	- Keys in each leaf strictly increasing; leaf traversal yields a strictly increasing global sequence.
	- Internal node child ranges are non-overlapping and cover all keys under that parent.
	- Duplicate inserts replace prior value without changing count.
	- Deletions remove a single mapping and adjust count accordingly; unknown deletes throw.
	- Search/indexer throws on empty tree or missing key as appropriate.
- Performance guidance:
	- Avoid unnecessary allocations and copying in hot paths; prefer contiguous arrays and reuse where safe.
	- Splits should minimize data movement; verify via microbenchmarks for typical and worst-case patterns.
	- Track p50/p95/p99 latencies for representative sizes; maintain historical baselines.

## Development Workflow & Quality Gates

- Properties-first workflow: write/extend a property → observe failure (red) → implement/minimize → pass (green)
	→ refactor with properties unchanged.
- Quality gates for PRs:
	1) Property-based tests present for new/changed behavior and all pass locally/CI.
	2) Core invariants covered by properties remain satisfied (no flaky properties allowed).
	3) Benchmark delta provided when perf could be affected; degradation >5% requires explicit approval.
	4) Public API changes documented with rationale and migration notes (if applicable).
- Plans/specs/tasks MUST include a “Constitution Check” mapping changes to the above gates.

## Governance

- This constitution supersedes other informal practices. Non-compliant changes MUST include an exception
	rationale and a plan to return to compliance.
- Amendment procedure:
	- Propose via PR updating this file with a Sync Impact Report summarizing changes and template impacts.
	- Reviewers verify alignment across plan/spec/tasks templates and README or other guidance.
	- Upon merge, update LAST_AMENDED_DATE and bump version per rules below.
- Versioning policy for this constitution:
	- MAJOR: Backward-incompatible shifts to principles or governance.
	- MINOR: New principles/sections or materially expanded guidance.
	- PATCH: Clarifications/typos with no behavioral/governance change.
- Compliance review expectations:
	- Code review MUST explicitly check Constitution Check sections in plans/specs/tasks.
	- CI SHOULD run properties and fast benchmarks where feasible; full benchmarks MAY run on a schedule.

**Version**: 1.0.0 | **Ratified**: 2025-10-19 | **Last Amended**: 2025-10-19
