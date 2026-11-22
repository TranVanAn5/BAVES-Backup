# DEL-04 — Elicitation Results

---

## v1 — Có ghi chú nhưng thiếu cấu trúc / thiếu conflict & assumption rõ

```text
# Elicitation Results — OmniMart Sprint 2 (v1)

## Session info
- Ngày: (điền)
- BA: Learner
- Participants: Quân (Ops), Yến (CS), Tuấn (IT)
- Deliverable: DEL-04 v1.0

## Mục tiêu
Hỏi về tồn kho và khiếu nại khách.

## Ghi chú phỏng vấn
- Ops nói vấn đề lệch tồn và hủy đơn.
- CS nói khách giận vì web còn hàng kho hết.
- IT nói hệ thống sync không phải lúc nào cũng realtime.

## Kết luận
Cần đồng bộ tốt hơn và quy tắc rõ hơn.
```

### Yếu v1
- Không tách Fact / Assumption / Open Question.
- Không ghi số liệu / khoảng số.
- Không nêu conflict giữa phòng ban.
- Không đủ làm đầu vào BRD chặt.

---

## v2 — Đầy đủ (sau khi đã hỏi đủ Turn Sprint 2)

```text
# Elicitation Results — OmniMart Sprint 2 (v2)

## 0. Session details
- Initiative: OmniMart omnichannel order
- Deliverable: DEL-04
- Version: v2.0
- Elicitation type: 1-1 interviews (ordered)
- Attendees: Trần Minh Quân (Ops), Lê Hoàng Yến (CS/Sales), Nguyễn Quốc Tuấn (IT)
- BA: Learner (Lead BA)

## 1. Objectives
1) Xác định nguồn ATP và thời điểm hold/release tồn cho đơn online.
2) Định lượng mức độ hủy đơn / ticket liên quan lệch tồn.
3) Làm rõ latency sync thực tế và ràng buộc IT.
4) Thu thập conflict ưu tiên giữa confirm đơn nhanh và kiểm soát tồn.

## 2. Findings register

### Facts (đã khẳng định trong hội thoại — điền đúng lời agent trả lời)
| ID | Fact | Source |
|----|------|--------|
| F1 | (vd) ATP/tồn online đang lấy từ … | Ops |
| F2 | Hold/release xảy ra khi … | Ops |
| F3 | Ticket “web còn kho hết” thuộc nhóm khiếu nại chính | CS |
| F4 | CS phải verify bằng … / chat kho | CS |
| F5 | Sync POS–WMS–ERP theo cơ chế … chu kỳ … | IT |
| F6 | IT không muốn thay core ERP trong phase này | IT |

### Assumptions
| ID | Assumption | Risk nếu sai |
|----|------------|--------------|
| A1 | Soft-limit flash sale có thể đặt theo SKU mà không đổi POS firmware | Scope IT phình |
| A2 | Canonical order-status service đủ cho CS/notify | CS vẫn phải tool riêng |

### Open questions
| ID | Question | Owner để resolve |
|----|----------|------------------|
| Q1 | Hold timeout chính thức (phút) là bao nhiêu? | Ops + Sales |
| Q2 | SLA latency tồn mục tiêu chốt ở mức nào? | Ops + IT |

## 3. Conflicts / tensions
1. **CS/Sales vs Ops:** muốn giữ trải nghiệm confirm/đặt nhanh vs cần ATP đáng tin.
2. **Business vs IT:** đòi “realtime” vs IT yêu cầu SLA cụ thể và không đập core.
3. **Ops vs CS:** thông báo khách có thể sai khi nguồn trạng thái chưa chuẩn hóa.

## 4. Quantitative signals (điền số agent đưa; nếu chỉ có khoảng thì ghi khoảng)
- Hủy đơn online vì hết hàng (ngày thường): …
- Ngày cao điểm: …
- Latency sync quan sát: …
- Chỉ số CS mong giảm: …

## 5. Implications for BRD (DEL-05)
- Cần rules: ATP gate trước confirm; hold timeout; canonical status; flash soft-limit; audit chỉnh tồn.
- Cần NFR latency + audit cho lớp đồng bộ/status.
```
