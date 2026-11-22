# Sprint 3 — Follow-up IT (Tuấn) — Event E1

**Mục đích:** Làm rõ nguyên nhân kỹ thuật khả dĩ, latency thực tế, flag/kill-switch khả thi; ràng buộc không đập core.

**Quy tắc:** Turn-by-Turn. Ops + CS xong mới hỏi IT (tránh đòi “realtime” mơ hồ).

---

### Turn 1 — Xác nhận sự cố kỹ thuật
> Anh Tuấn, phía IT quan sát E1 thế nào: queue backlog, job sync chậm, hay mismatch source? Em cần phân biệt symptom vs root cause anh đang nghi.

### Turn 2 — Latency thực tế
> Latency / freshness tồn đang ở mức nào (p95 hoặc chu kỳ quan sát) so với SLA đã nói ở Sprint 2?

### Turn 3 — Detect stale
> Hệ thống có (hoặc có thể thêm nhanh) **signal stale** không — timestamp last sync, lag threshold, health của pipeline?

### Turn 4 — Kill-switch / soft controls
> Có thể bật tạm: block confirm khi lag > X, ẩn ATP online, hoặc pause soft-limit campaign — lớp nào làm được **không đụng ERP core**?

### Turn 5 — Effort & risk
> Với 1–2 control Ops/CS vừa ưu tiên, effort IT ước lượng (ngày/người) và rủi ro regression?

### Turn 6 — Logging / audit khi sự cố
> Khi override tay hoặc pause sync, audit/log có đủ để post-mortem không? Thiếu field nào?

### Turn 7 — Đề xuất delta NFR
> Anh đề xuất chỉnh NFR thế nào cho đợt này: ngưỡng alert lag, hành vi khi stale, hay chỉ monitoring trước?

---

## Ghi nhanh sau IT
- Root cause nghi / đã confirm:
- Latency vs SLA:
- Control khả thi ngắn hạn:
- Effort:
- Delta NFR đề xuất:
