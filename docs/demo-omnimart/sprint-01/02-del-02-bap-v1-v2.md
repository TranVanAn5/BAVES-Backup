# DEL-02 — Business Analysis Plan (BAP)

Mở Docs → chọn template **DEL-02** → dán đúng các mục skeleton hệ thống → **Nộp**.

Template hệ thống (BABOK Planning & Monitoring):

0. Document Governance  
1. Business Analysis Approach  
2. Stakeholder Engagement Plan  
3. Business Analysis Governance  
4. Business Analysis Information Management  
5. BA Activities, Deliverables & Schedule  
6. Risks & Assumptions (BA Work)  
7. Business Analysis Performance Metrics  

> Skeleton yêu cầu nội dung **English**. Chỉ liệt kê DEL cần cho OMNI-001 (không ép đủ DEL-01..08). Chi tiết Power/Interest để **DEL-03**.

---

## v1 — Đủ khung nhưng còn sơ / chưa hợp lý

**Nộp lần 1** (trước hoặc ngay sau phỏng vấn nhẹ Sprint 1).

```text
# Business Analysis Plan

## Document Governance
- Document type: Business Analysis Planning and Monitoring
- Project: OmniMart Omnichannel Order | Plan ID: BAP-OMNI-2026-01
- Author: [Learner full name] (Lead Business Analyst) | Version: v1.0 | Deliverable: DEL-02
- Purpose of this BAP: Plan how BA work will be done for OmniMart order and inventory issues.

## Learner guidelines
1. Sections completed in English.
2. Only scenario deliverables in Section 5.
3. Deep stakeholder profiles belong in DEL-03.

## 1. Business Analysis Approach
Selected approach: Adaptive

Justification: We will interview people and write documents each sprint.

- Definition phase (early): talk to stakeholders
- Delivery phase (later sprints): write BRD and SRS
- Prioritization approach: (not decided)

## 2. Stakeholder Engagement Plan
| Stakeholder group | Engagement objective | Techniques | Cadence / Channel |
|-------------------|----------------------|------------|-------------------|
| Ops | Understand inventory | Interview | Sometime |
| CS | Understand complaints | Interview | Sometime |
| IT | Understand systems | Interview | Sometime |

## 3. Business Analysis Governance
- Requirements approval authority: Sponsor
- Prioritization authority: (TBD)
- Change Request (CR) process: (TBD)
- Impact Analysis SLA: (TBD)
- Change Control Board (CCB): (TBD)
- Escalation path: (TBD)
- Definition of Approved vs Deferred: (TBD)

## 4. Business Analysis Information Management
- Repository / tools: Docs in BA-VES
- Naming & versioning: (TBD)
- Access control: (TBD)
- Traceability path: (TBD)
- Single source of truth: (TBD)

## 5. BA Activities, Deliverables & Schedule
| BA activity | Deliverable / code | Owner | Sprint / milestone | Acceptance check |
|-------------|-------------------|-------|--------------------|------------------|
| Plan BA work | DEL-02 BAP | Lead BA | Sprint 1 | Submitted |
| Stakeholder list | DEL-03 | Lead BA | Sprint 1 | Submitted |
| Elicitation | DEL-04 | Lead BA | Sprint 2 | Submitted |
| BRD | DEL-05 | Lead BA | Sprint 2 | Submitted |
| SRS | DEL-07 | Lead BA | Sprint 3 | Submitted |
| RTM | DEL-08 | Lead BA | Sprint 4 | Submitted |

## 6. Risks & Assumptions (BA Work)
| Risk / assumption | Type | Impact | Mitigation / response |
|-------------------|------|--------|------------------------|
| Hard to get numbers | Risk | M | Ask again later |
| Scope stays small | Assumption | M | (none) |

## 7. Business Analysis Performance Metrics
| BA metric | Target | Measurement method | Review cadence |
|-----------|--------|--------------------|----------------|
| Documents submitted | All required DELs | Count submitted files | End of scenario |

BA approach improvement review: After final sprint.
```

### Chỗ cố ý yếu ở v1
- Approach thiếu justification / formality / MoSCoW rõ.
- Engagement thiếu objective & cadence cụ thể theo thứ tự Ops→CS→IT.
- Governance gần như TBD.
- Information management / traceability trống.
- Risks không có mitigation mạnh; metrics không đo chất lượng BA.

---

## v2 — Đầy đủ, bám skeleton (nộp sau phỏng vấn nhẹ Sprint 1)

```text
# Business Analysis Plan

## Document Governance
- Document type: Business Analysis Planning and Monitoring (BABOK v3)
- Project: OmniMart — Omnichannel Order & Inventory Availability (OMNI-001) | Plan ID: BAP-OMNI-2026-01
- Author: [Learner full name] (Lead Business Analyst) | Version: v2.0 | Deliverable: DEL-02
- Purpose of this BAP: Define the BA approach, stakeholder engagement sequence, governance, information management, sprint deliverables (DEL-02/03/04/05/07/08 only), BA risks, and performance metrics for reducing cancelations and complaints caused by inconsistent ATP/inventory and order status across channels.

## Learner guidelines
1. Sections completed in English.
2. Only OMNI-001 required deliverables in Section 5 (no DEL-01, no DEL-06).
3. Power/Interest and attitude detail live in DEL-03; this BAP covers engagement planning only.

## 1. Business Analysis Approach
Selected approach: Hybrid (lightweight predictive framing + adaptive delivery per sprint)

Justification: OmniMart needs an early shared problem/scope baseline (predictive artifacts: BAP, stakeholder map, then BRD) while sprint events (E1 sync lag, E2 flash sale) require adaptive backlog updates to SRS/RTM without rewriting ERP core.

- Definition phase (early — Sprint 1–2): Formal enough for interview scripts, elicitation register, and testable BR statements; predictive artifacts DEL-02, DEL-03, DEL-04, DEL-05.
- Delivery phase (later — Sprint 3–4): Adaptive response to events; refine solution requirements (DEL-07 SRS) and bidirectional traceability (DEL-08 RTM); update BRD only via governed change.
- Prioritization approach: MoSCoW on business rules and solution requirements; business value = reduce inventory-related cancelations and CS tickets first; technical constraints from IT gate “Must” items.

## 2. Stakeholder Engagement Plan
| Stakeholder group | Engagement objective | Techniques | Cadence / Channel |
|-------------------|----------------------|------------|-------------------|
| Operations (Trần Minh Quân) | Confirm as-is ATP/hold/release, cancelation drivers, peaking pain | Structured 1-1 interview (ordered turns) | Sprint 1 light; Sprint 2 deep; Sprint 3–4 event follow-ups / in-sim chat |
| CS / Sales (Lê Hoàng Yến) | Quantify “web in stock / warehouse out” tickets; status/notify gaps; flash surge | Structured 1-1 interview | After Ops each wave; same channels |
| IT (Nguyễn Quốc Tuấn) | Sync latency reality, integration constraints, feasible controls without ERP rewrite | Structured 1-1 interview | After Ops+CS each wave so asks are SLA-specific |
| Sponsor / COO (as needed) | Approve scope, priority, CR decisions | Written summary + Review Sprint outcomes | End of each sprint / on CR |

Note: Authority, attitude, influence → DEL-03. Interview order is always Ops → CS → IT unless an event forces a targeted follow-up.

## 3. Business Analysis Governance
- Requirements approval authority: Sponsor (COO) for Must scope; Lead BA owns draft quality; Ops/CS/IT confirm domain facts.
- Prioritization authority: Sponsor with Ops (inventory integrity) and Sales (campaign risk) input; IT veto on infeasible “realtime” without SLA.
- Change Request (CR) process: After baseline DEL-05, event-driven deltas documented in sprint Review + updated DEL-07/08 (and BRD amendment note if BR text changes).
- Impact Analysis SLA: Within 48 hours of event (E1/E2) or material stakeholder conflict.
- Change Control Board (CCB): Lead BA (facilitator), Ops, CS/Sales, IT; decision rule = majority with Sponsor break-tie on Must items.
- Escalation path: Unresolved Ops–IT conflict on latency/ATP → Sponsor within same sprint.
- Definition of Approved vs Deferred: Approved = testable, in-scope, owner named, acceptance check defined; Deferred = out of ERP-core rewrite, UI redesign, pricing engine, or no owner/SLA.

## 4. Business Analysis Information Management
- Repository / tools: BA-VES Docs (authoritative drafts), sprint Review feedback, optional personal notepad for raw notes.
- Naming & versioning: `{DEL-xx}-{short-name}-v{n}`; v1 = complete-but-weak baseline; v2 = corrected after interviews/events; never delete prior Review evidence/snapshots.
- Access control: Learner edits own docs; Review Sprint consumes submitted versions; do not treat chat transcripts as SoT.
- Traceability path: Business need / evidence (elicitation) → DEL-05 BR → DEL-07 SRS (FR/NFR/US+AC) → DEL-08 RTM → tests/acceptance checks.
- Single source of truth: Approved DEL-05 for business requirements; approved DEL-07 for solution requirements; DEL-08 for links; DEL-04 for elicitation evidence.

## 5. BA Activities, Deliverables & Schedule
Include only deliverables required for OMNI-001.

| BA activity | Deliverable / code | Owner | Sprint / milestone | Acceptance check |
|-------------|-------------------|-------|--------------------|------------------|
| Plan BA work & engagement sequence | DEL-02 BAP | Lead BA | Sprint 1 | All 7 sections filled; approach + governance not TBD |
| Stakeholder analysis (power/interest, RACI-lite) | DEL-03 | Lead BA | Sprint 1 | Named personas Ops/CS/IT; conflicts noted |
| Deep elicitation (facts/assumptions/conflicts) | DEL-04 | Lead BA | Sprint 2 (after Ops→CS→IT) | Fact table + open questions + quantitative signals |
| Business requirements & rules | DEL-05 BRD | Lead BA | Sprint 2 | Testable BRs + metrics + NFR latency/audit |
| Solution requirements after E1 (and updates) | DEL-07 SRS | Lead BA | Sprint 3 | FR/NFR/US+AC mapped to BRs; stale/lag controls if E1 |
| Bidirectional traceability after E2 | DEL-08 RTM | Lead BA | Sprint 4 | Objectives→BR→SRS→AC covered for Must items |

## 6. Risks & Assumptions (BA Work)
| Risk / assumption | Type | Impact | Mitigation / response |
|-------------------|------|--------|------------------------|
| Stakeholders give vague volumes (“a lot of tickets”) | Risk | H | Ask for ranges; log Assumption IDs in DEL-04; follow-up same sprint |
| IT rejects “realtime” without numeric SLA | Risk | H | Force Ops/CS to state acceptable lag; capture as NFR candidate before SRS |
| Scope creep into ERP/WMS core rewrite | Risk | H | Keep out-of-scope explicit; CCB rejects core rewrite Musts |
| Event E1/E2 arrives before BRD baseline quality | Risk | M | Still run ordered follow-ups; document temporary controls vs permanent BR changes |
| Scope baseline remains stable through Sprint 1 | Assumption | M | If false after interviews, revise BAP Section 5 and re-Review |
| Soft-limit for flash can sit above ERP (orchestration/API) | Assumption | H | Validate with IT in Sprint 2/4; if false, defer soft-limit automation |

## 7. Business Analysis Performance Metrics
| BA metric | Target | Measurement method | Review cadence |
|-----------|--------|--------------------|----------------|
| First-pass Review usability (v1→v2 needed only for intentional gaps) | v2 clears prior Review blockers | Compare Review Sprint feedback items closed | End of each sprint |
| Ordered engagement compliance | 100% Ops→CS→IT before writing DEL-04/05 | Checklist in sprint script | Sprint 2, 3, 4 |
| Traceability completeness (Must BRs) | 100% Must BRs linked in DEL-08 | Count BR IDs with SRS + AC links | Sprint 4 |
| Open question burn-down | Critical Qs closed or owned before UAT-style Review | DEL-04 open-question register | Sprint 2–3 |
| Assumption explicitness | Every material unknown has Assumption ID | Spot-check DEL-04/05 | Sprint 2 Review |

BA approach improvement review: After Sprint 2 Review (adjust interview depth) and after Sprint 4 final Review (inspect hybrid approach fit).
```

### Sau khi nộp v2
Quay [`README.md`](./README.md) → Review Sprint 1 (nếu chưa) → làm DEL-03 → Sprint 2.
