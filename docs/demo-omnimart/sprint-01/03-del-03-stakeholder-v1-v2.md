# DEL-03 — Stakeholder Analysis

Docs → **DEL-03** → dán → Nộp.

---

## v1 — Có danh sách nhưng thiếu chiều sâu

```text
# Stakeholder Analysis — OmniMart (v1)

## 0. Governance
- Document: Stakeholder Register
- Version: v1.0
- Deliverable: DEL-03

## 1. Danh sách stakeholder
1. Trần Minh Quân — Operations Manager — quan tâm kho và đơn hàng
2. Lê Hoàng Yến — CS Lead — quan tâm khách hàng
3. Nguyễn Quốc Tuấn — IT Lead — quan tâm hệ thống

## 2. Ảnh hưởng
Cả ba đều quan trọng.

## 3. Kế hoạch liên lạc
Sẽ phỏng vấn khi cần.

## 4. RACI
(Chưa làm)
```

### Yếu ở v1
- Không có power/interest, goals/pain tách bạch.
- Không có chiến lược engagement.
- RACI trống → AI có thể góp ý thiếu.

---

## v2 — Đầy đủ hơn (sau phỏng vấn Sprint 1)

```text
# Stakeholder Analysis — OmniMart (v2)

## 0. Governance
- Project: OmniMart Omnichannel Order
- Version: v2.0
- Deliverable: DEL-03
- Linked BAP: DEL-02 v2

## 1. Stakeholder register

| Name | Role / Dept | Goals | Pain points | Power | Interest | Attitude |
|------|-------------|-------|-------------|-------|----------|----------|
| Trần Minh Quân | Operations Manager / Vận hành | Giảm đơn trùng; rõ thời điểm giữ/trả tồn; ít thao tác thêm | Đối soát tay giữa hệ thống; ngại quy trình nặng | High | High | Cautious supporter |
| Lê Hoàng Yến | Sales/CS Lead / Kinh doanh & CSKH | Thông báo đúng lúc; giảm khiếu nại “web còn kho hết” | Hotline quá tải; thông báo sai/trễ | High | High | Supportive |
| Nguyễn Quốc Tuấn | IT Lead / CNTT | Toàn vẹn dữ liệu; tích hợp không đập core | Kinh doanh đòi realtime thiếu SLA; rủi ro bảo mật API | High | Medium-High | Guarded |

## 2. Power–Interest & engagement
- Quân, Yến: Manage Closely — interview sâu + review artifact.
- Tuấn: Keep Satisfied / Collaborate on constraints — interview kỹ thuật + xác nhận feasibility.

## 3. RACI (BA activities)

| Activity | Lead BA | Quân (Ops) | Yến (CS) | Tuấn (IT) |
|----------|---------|------------|----------|-----------|
| Elicitation interviews | A/R | C | C | C |
| Business rules catalog | A/R | C | C | C |
| Order status / CX rules | A/R | I | C | C |
| Integration constraints | A/R | I | I | C |
| Sprint review artifacts | A/R | C | C | C |

A=Accountable R=Responsible C=Consulted I=Informed

## 4. Kế hoạch phỏng vấn Sprint 2 (chuẩn bị)
1. Ops: ATP, hold/release, điểm lệch tồn, chỉ số hủy đơn.
2. CS: ticket pattern, nguồn thông báo, kỳ vọng trạng thái khách thấy.
3. IT: cơ chế sync, latency thực tế, ràng buộc không đập core, điều kiện “gần realtime”.

## 5. Risks stakeholder
- Conflict Sales confirm nhanh vs Ops/IT kiểm soát tồn → cần làm lộ trong DEL-04.
- IT có thể block scope nếu thiếu số liệu SLA → bắt buộc follow-up định lượng.
```
