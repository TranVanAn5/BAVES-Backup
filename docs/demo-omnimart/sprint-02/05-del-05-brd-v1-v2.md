# DEL-05 — Business Requirements Document (BRD)

Dùng sau khi có DEL-04 v2. Submit Review Sprint khi xong BRD v2.

---

## v1 — Đầy đủ khung nhưng còn thiếu / chưa hợp lý

```text
# BRD — OmniMart Omnichannel Order Sync (v1)

## 1. Purpose
Cải thiện đồng bộ tồn và trải nghiệm đặt hàng online.

## 2. Business problem
Khách thấy còn hàng nhưng kho hết → hủy đơn / khiếu nại.

## 3. Objectives
- Giảm hủy đơn vì hết hàng
- Giảm ticket CS
- Đồng bộ tốt hơn giữa kênh

## 4. Stakeholders
Ops, CS, IT, Sales

## 5. Scope
In scope:
- Đồng bộ tồn online
- Thông báo trạng thái đơn rõ hơn

Out of scope:
- Đổi ERP core

## 6. Business requirements
BR-01: Hệ thống phải hiển thị tồn chính xác hơn.
BR-02: CS phải biết trạng thái đơn thật.
BR-03: Flash sale phải có kiểm soát.

## 7. Success metrics
- Giảm hủy đơn
- Giảm ticket

## 8. Risks
- IT resource
- Đổi quy trình Ops
```

### Yếu v1
- BR mơ hồ (“chính xác hơn”) — không testable.
- Thiếu ATP/hold/timeout/canonical status/soft-limit/audit cụ thể.
- Thiếu NFR latency / audit.
- Success metrics không có baseline/target.
- Không map từ elicitation findings.

---

## v2 — Đầy đủ & hợp lý hơn (sau elicitation)

```text
# BRD — OmniMart Omnichannel Order Availability & Status (v2)

## Document control
- Deliverable: DEL-05
- Version: v2.0
- Initiative: OMNI-001 OmniMart
- Based on: DEL-02/03 + DEL-04 elicitation
- BA: Learner

## 1. Purpose
Định nghĩa yêu cầu nghiệp vụ để (a) giảm đơn online xác nhận khi không còn ATP thực, (b) chuẩn hóa trạng thái đơn cho CS/khách, (c) kiểm soát soft-limit flash sale — trong phạm vi không thay ERP core.

## 2. Business problem
- Nguồn ATP/tồn online chưa tin cậy tại thời điểm confirm → hủy / oversell cảm nhận.
- CS thiếu nguồn trạng thái chuẩn → verify thủ công, phản hồi chậm/sai.
- Flash sale / peaking tăng lệch tồn và ticket.
- Sync đa hệ thống có latency; “realtime tuyệt đối” không khả thi nếu không có SLA.

## 3. Goals & success metrics
| Goal | Metric | Baseline (từ elicitation) | Target (đề xuất BA) |
|------|--------|---------------------------|---------------------|
| G1 Giảm hủy vì hết hàng | % / số đơn hủy/ngày liên quan inventory | … | Giảm ≥ X% sau go-live + 4 tuần |
| G2 Giảm ticket tồn lệch | Ticket/ngày nhóm “web còn kho hết” | … | Giảm ≥ Y% |
| G3 Rõ trạng thái đơn | % ticket cần chat kho thủ công | … | Giảm ≥ Z% |
| G4 Kiểm soát flash | Soft-limit breaches / peaking incident | … | Soft-limit + alert hoạt động 100% campaign có cấu hình |

## 4. Stakeholders & owners
| Role | Interest | Decision |
|------|----------|----------|
| Ops (Quân) | ATP, hold, inventory integrity | Co-owner business rules inventory |
| CS (Yến) | Status truth, ticket load | Co-owner customer-facing status |
| IT (Tuấn) | Sync, SLA, architecture fit | Owner technical feasibility |
| Sales | Flash volume vs risk | Soft-limit policy co-owner |
| Sponsor | Outcome & budget | Final priority |

## 5. Scope

### In scope
1. ATP gate trước confirm đơn online (theo nguồn ATP đã thống nhất).
2. Hold / timeout / release tồn theo rule nghiệp vụ.
3. Canonical order status service/view cho CS (+ nguồn notify khách).
4. Soft-limit / quota SKU cho flash / peaking (config + alert).
5. Audit trail chỉnh tồn / override liên quan đơn.
6. NFR: latency sync / freshness tồn; availability lớp status (mức SLA).

### Out of scope (phase này)
1. Thay thế / rewrite ERP core.
2. Đổi firmware toàn bộ POS.
3. Loyalty redesign / CMS full rewrite.
4. Realtime tuyệt đối mọi SKU mọi kênh nếu vượt SLA đã chốt.

### Dependencies
- Nguồn master tồn / ATP đã chọn (WMS/ERP/aggregate — theo elicitation).
- Integration capacity IT; config soft-limit owner.

## 6. Business requirements (testable)

| ID | Requirement | Priority | Source |
|----|-------------|----------|--------|
| BR-01 | Trước confirm đơn online, hệ thống phải kiểm tra ATP theo nguồn đã định; nếu ATP không đủ thì không confirm / phải có path từ chối rõ. | Must | Ops |
| BR-02 | Khi reserve tồn thành công phải tạo hold với timeout T phút (T chốt với Ops/Sales); hết hạn phải release và ghi audit. | Must | Ops |
| BR-03 | Mọi thay đổi trạng thái đơn nghiệp vụ phải cập nhật canonical order status; CS xem một nguồn chuẩn, không dựa chat kho làm nguồn chính. | Must | CS |
| BR-04 | Thông báo khách (email/app) lấy từ canonical status hoặc mapping đã duyệt; cấm thông báo mâu thuẫn với status chuẩn. | Must | CS |
| BR-05 | Campaign flash/peaking: soft-limit theo SKU (hoặc nhóm) cấu hình được; khi đạt ngưỡng phải block/throttle + alert Ops/Sales. | Must | Ops/Sales/IT |
| BR-06 | Override / chỉnh tồn ảnh hưởng đơn phải có user, lý do, timestamp (audit). | Must | Ops/IT |
| BR-07 | Latency / freshness tồn online phải đáp ứng SLA đã thỏa thuận với IT (ghi rõ p95 hoặc chu kỳ sync mục tiêu). | Must | IT/Ops |
| BR-08 | Không yêu cầu thay ERP core để đạt BR-01..07 trong phase này. | Must (constraint) | IT |

## 7. Assumptions & constraints
- Soft-limit triển khai được ở lớp orchestration/API mà không đụng POS firmware.
- Canonical status có thể bootstrap từ event đơn + inventory signal hiện có.
- Open Q (hold timeout, số SLA cụ thể) phải đóng trước UAT nếu chưa đóng ở elicitation.

## 8. Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| “Realtime” kỳ vọng vs SLA thực | Scope creep / fail UAT | Chốt NFR số trong BR-07 |
| Soft-limit config sai | Mất doanh thu hoặc vẫn oversell | Owner Sales+Ops; alert |
| CS adoption thấp | Vẫn chat kho | Training + UI status rõ |
| Integration delay | Trễ go-live | Slice MVP: ATP gate + status trước soft-limit nâng cao |

## 9. Acceptance (business)
- Demo: đơn không confirm khi ATP=0 theo rule.
- Demo: hold hết hạn → release.
- Demo: CS thấy status khớp trạng thái sau cancel/fulfill.
- Demo: soft-limit chặn khi đạt ngưỡng cấu hình.
- Metrics G1–G3 đo được sau go-live theo dashboard đã định.
```

### Sau khi paste BRD v2
1. Review Sprint → xem grade / feedback.
2. Nếu AI bảo thiếu NFR/metrics → chỉnh 1 lần rồi Review lại (nếu cho phép) hoặc ghi nhận cho Sprint 3.
3. Sang Sprint 3 khi event E1 bật.
