# DEL-07 — Change Request / Requirements Delta (sau E1)

---

## v1 — Có mô tả sự cố nhưng thiếu delta testable

```text
# Change Request — E1 Sync Delay (v1)

## Summary
Có độ trễ đồng bộ tồn khiến đơn và CS bị ảnh hưởng. Cần xử lý gấp.

## Impact
- Ops phải xử lý tay
- CS tăng ticket
- Khách không hài lòng

## Proposed change
Cải thiện sync và cảnh báo khi chậm.

## Priority
High
```

### Yếu v1
- Không trích dẫn BR cũ bị ảnh hưởng (BR-01, BR-07…).
- Không có hành vi cụ thể khi stale.
- Không có acceptance / rollback.
- Không phân temporary vs permanent.

---

## v2 — Đầy đủ sau follow-up Ops → CS → IT

```text
# DEL-07 — Change Request: E1 Inventory Sync Lag Controls (v2)

## Document control
- Deliverable: DEL-07
- Version: v2.0
- Trigger: Event E1 (manual / observed sync delay)
- Based on: DEL-05 BRD v2 + Sprint 3 elicitation follow-ups
- BA: Learner

## 1. Event summary
- Symptom (Ops): …
- Symptom (CS): ticket … tăng …
- IT observation: latency … vs SLA …; suspected cause …

## 2. Business impact
| Area | Impact | Evidence |
|------|--------|----------|
| Orders | Confirm sai / hủy / hold tay | Ops |
| CS | Ticket surge; verify chậm | CS |
| Trust | Thông báo / “còn hàng” sai khi stale | CS |

## 3. Requirements impacted (from DEL-05)
| BR | Impact | Action |
|----|--------|--------|
| BR-01 ATP gate | Gate có thể dựa data stale | Bổ sung stale policy |
| BR-03/04 Status & notify | Notify có thể sai | Pause/guard notify khi stale |
| BR-07 Latency NFR | Đang breach hoặc gần breach | Alert + threshold hành vi |

## 4. Change items (testable)

### Temporary (hot-fix / ops control) — ưu tiên sprint này
| ID | Change | Owner |
|----|--------|-------|
| CR-T1 | Khi `inventory_lag > L` (L = … theo IT), **không confirm** đơn online với ATP từ nguồn stale; hiển thị lý do nghiệp vụ chuẩn. | Ops+IT |
| CR-T2 | CS tool hiển thị **stale banner** + last_sync timestamp khi lag > L. | CS+IT |
| CR-T3 | Auto customer notify liên quan “còn hàng / sẵn sàng” **tạm pause** hoặc gắn disclaimer nội bộ khi stale. | CS+IT |
| CR-T4 | Kill-switch: pause soft-confirm / peaking SKU list do Ops bật (không đụng ERP core). | Ops+IT |

### Permanent (backlog sau ổn định)
| ID | Change |
|----|--------|
| CR-P1 | Alert p95 lag + runbook on-call |
| CR-P2 | NFR BR-07 cập nhật ngưỡng alert & hành vi khi breach |
| CR-P3 | Post-mortem template + audit đủ field override |

## 5. Out of scope this CR
- Rewrite sync engine / thay ERP core
- Cam kết realtime tuyệt đối

## 6. Acceptance criteria
1. Demo: giả lập lag > L → confirm bị chặn / path từ chối rõ.
2. Demo: CS thấy stale banner + timestamp.
3. Demo: notify guard hoạt động khi stale.
4. Kill-switch Ops bật/tắt được và có audit user+time.
5. Metrics 24–48h: ticket nhóm lệch tồn giảm về ngưỡng CS đã nói **hoặc** workaround được ghi nhận có owner.

## 7. Rollback
- Tắt stale-block nếu false positive quá cao; giữ monitoring; escalate sponsor nếu doanh thu bị ảnh hưởng vượt ngưỡng Sales/Ops thỏa thuận.

## 8. Open questions
- Giá trị L chính thức: …
- Ai approve bật kill-switch ngoài giờ: …
```

### Sau DEL-07 v2
Review Sprint → sang Sprint 4 khi E2 (Flash Sale) xuất hiện.
