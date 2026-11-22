# Sprint 4 — IT (Tuấn) — Event E2 Flash Sale

**Mục đích:** Khả thi soft-limit, rate limit, capacity, NFR peaking; không đập core.

**Quy tắc:** Turn-by-Turn sau Ops + CS.

---

### Turn 1 — Capacity
> Anh Tuấn, hạ tầng hiện tại chịu peaking flash mức Ops mô tả được không? Bottleneck ở sync, order API, hay DB?

### Turn 2 — Soft-limit placement
> Soft-limit nên đặt ở lớp nào (orchestration / API gateway / inventory service) để **không** sửa ERP core / POS firmware?

### Turn 3 — Config & safety
> Config soft-limit cần ai approve, audit thế nào, và có dry-run / shadow mode trước giờ G không?

### Turn 4 — Behavior on limit
> Khi chạm limit: HTTP/business error code thế nào; idempotency confirm; race nhiều request cùng SKU xử lý ra sao ở mức anh chấp nhận được?

### Turn 5 — NFR peaking
> Anh đề xuất NFR cho cửa sổ flash: throughput mục tiêu, error budget, latency p95 order confirm, lag inventory tối đa?

### Turn 6 — Tương thích DEL-07
> Stale-block và soft-limit cùng bật: thứ tự ưu tiên rule (eval order) anh muốn ghi trong SRS?

### Turn 7 — Slice giao Sprint này
> Phần nào Must ship trước giờ sale; phần nào để phase sau?

---

## Ghi nhanh
- Bottleneck:
- Soft-limit layer:
- Race / error semantics:
- NFR peaking:
- Eval order vs stale:
- Must vs later:
