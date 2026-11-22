# DEL-08 — Software / Solution Requirements (SRS) — Flash + omnichannel controls

---

## v1 — Có mục nhưng thiếu rule/NFR/eval order

```text
# SRS — OmniMart Flash Sale Support (v1)

## 1. Introduction
Hỗ trợ flash sale và đồng bộ đơn hàng tốt hơn.

## 2. Functional requirements
- FR1: Hệ thống hỗ trợ soft-limit.
- FR2: Hệ thống gửi thông báo đúng.
- FR3: CS xem được trạng thái đơn.

## 3. Non-functional
- Hệ thống phải nhanh và ổn định khi sale.

## 4. Constraints
- Không đổi ERP core.
```

### Yếu v1
- FR không testable; thiếu ATP/hold/stale/soft-limit chi tiết.
- NFR không có số.
- Không có rule evaluation order.
- Không map BR/CR → FR.
- Thiếu acceptance / error cases.

---

## v2 — Đầy đủ (map BRD + DEL-07 + E2 interviews)

```text
# DEL-08 — SRS: Order Availability, Status & Flash Soft-Limit (v2)

## Document control
- Deliverable: DEL-08
- Version: v2.0
- Inputs: DEL-05 BRD v2, DEL-07 CR v2, Sprint 4 interviews
- BA: Learner

## 1. Goals
Thực thi business rules ATP/hold/status/soft-limit + stale guards trong kiến trúc không thay ERP core; chịu được peaking flash theo NFR đã chốt với IT.

## 2. Traceability
| Business / Change | SRS IDs |
|-------------------|---------|
| BR-01 ATP gate | FR-ATP-01 |
| BR-02 Hold timeout | FR-HLD-01..02 |
| BR-03/04 Canonical status & notify | FR-STA-01..02 |
| BR-05 Soft-limit | FR-SL-01..03 |
| BR-06 Audit | FR-AUD-01 |
| BR-07 / CR latency | NFR-LAG-01, FR-STALE-01 |
| CR-T1..T4 | FR-STALE-01..02, FR-KS-01 |

## 3. Functional requirements

### ATP & confirm
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-ATP-01 | Trước confirm online, service phải đọc ATP từ nguồn đã định; nếu ATP < qty → reject với business code `ATP_INSUFFICIENT`. | Must |
| FR-STALE-01 | Nếu `inventory_lag > L`, coi ATP là stale: reject confirm với `ATP_STALE` (hoặc path chờ) — **ưu tiên hơn** soft-limit message. | Must |
| FR-HLD-01 | Confirm thành công tạo hold `(sku, qty, orderId, expiresAt)`. | Must |
| FR-HLD-02 | Job release hold khi hết hạn hoặc cancel; ghi audit. Trong cửa sổ flash, timeout mặc định = T_flash (≤ T thường) nếu Ops đã chốt. | Must |

### Soft-limit (flash)
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-SL-01 | Soft-limit config theo SKU/nhóm: `limitQty` hoặc `limitOrders` + window; chỉ user role được phép sửa; mọi đổi có audit. | Must |
| FR-SL-02 | Khi remaining soft-limit < qty request → reject `SOFT_LIMIT_REACHED` (không silent drop). | Must |
| FR-SL-03 | Race: dùng atomic decrement / conditional update; không cho vượt limit quá sai số đã thỏa thuận với IT (ghi rõ). | Must |

### Status & notify
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-STA-01 | Canonical status store/API là nguồn CS; map trạng thái nghiệp vụ đã duyệt. | Must |
| FR-STA-02 | Notify khách chỉ emit khi status transition hợp lệ; khi stale hoặc soft-limit reject, không gửi “còn hàng”. | Must |
| FR-STALE-02 | CS UI: banner stale + `last_sync_at` khi lag > L. | Must |

### Controls
| ID | Requirement | Priority |
|----|-------------|----------|
| FR-KS-01 | Kill-switch Ops: pause soft-confirm / pause peaking SKU list; audit user+time; không đụng ERP core. | Must |
| FR-AUD-01 | Override tồn / sửa limit / bật kill-switch: user, reason, before/after, timestamp. | Must |

### Rule evaluation order (bắt buộc ghi)
1. Kill-switch active? → block
2. Stale lag > L? → `ATP_STALE`
3. ATP đủ? → else `ATP_INSUFFICIENT`
4. Soft-limit remaining? → else `SOFT_LIMIT_REACHED`
5. Create hold + confirm + status update

## 4. Non-functional requirements
| ID | NFR | Target (điền số đã chốt với IT) |
|----|-----|----------------------------------|
| NFR-PERF-01 | p95 latency confirm API trong flash window | … ms |
| NFR-CAP-01 | Sustained throughput confirm | … req/s hoặc đơn/phút |
| NFR-LAG-01 | Alert khi inventory lag > L; page on-call nếu > L2 | L=…; L2=… |
| NFR-AVAIL-01 | Canonical status read availability | …% / cửa sổ |
| NFR-AUDIT-01 | Audit events durable ≥ N ngày | N=… |

## 5. Constraints
- Không rewrite ERP core / POS firmware (phase này).
- Soft-limit & stale controls ở orchestration/API layer.

## 6. Error catalog (tối thiểu)
| Code | Actor message intent | CS guidance |
|------|----------------------|-------------|
| ATP_INSUFFICIENT | Không đủ tồn | Không hứa còn hàng |
| ATP_STALE | Đang đồng bộ | Dùng script stale |
| SOFT_LIMIT_REACHED | Hết suất sale | Message soft-limit đã duyệt |

## 7. Acceptance (solution)
1. Test peaking: chạm soft-limit → đúng code; không oversell quá sai số đã chốt.
2. Test stale + flash: stale thắng soft-limit trong eval order.
3. Hold hết hạn flash → release + audit.
4. CS banner + notify guard.
5. Kill-switch on/off + audit.
6. NFR đo được trên môi trường test/staging theo IT.

## 8. Out of scope
- Loyalty redesign; CMS rewrite; realtime tuyệt đối mọi kênh.
```

### Sau DEL-08 v2
Review Sprint lần cuối demo → đối chiếu grade / snapshot các sprint trước.
