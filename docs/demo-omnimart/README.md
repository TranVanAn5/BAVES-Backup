# Bộ kịch bản demo chi tiết — OmniMart (OMNI-001)

Bộ này thay cho bản tổng quan cũ: có **thứ tự câu hỏi theo lượt**, **nội dung Docs v1 (còn lệch) → v2 (đủ)**, và checklist kiểm thử luồng.

## Cách dùng

1. Đọc [`00-setup-and-overview.md`](./00-setup-and-overview.md) — đăng nhập, start session, quy ước nộp/Review.
2. Làm lần lượt **Sprint 01 → 04** theo thư mục tương ứng.
3. Trong mỗi sprint:
   - Hỏi stakeholder **đúng thứ tự Turn** (không nhảy list).
   - Dán Docs **v1 → Nộp → Review** (hoặc phỏng vấn xong mới nộp, tùy bước trong file sprint).
   - Sửa thành **v2 → Nộp lại** khi file yêu cầu (để có version + so sánh cải thiện).
4. Kết thúc bằng [`99-verification-checklist.md`](./99-verification-checklist.md).

## Cấu trúc

| Thư mục / file | Nội dung |
|----------------|----------|
| [`00-setup-and-overview.md`](./00-setup-and-overview.md) | Setup, tài khoản, DEL theo sprint, quy tắc demo |
| [`sprint-01/`](./sprint-01/) | Kế hoạch BA: phỏng vấn nhẹ + DEL-02/03 v1→v2 |
| [`sprint-02/`](./sprint-02/) | Elicitation sâu theo thứ tự Ops→CS→IT + DEL-04/05 v1→v2 |
| [`sprint-03/`](./sprint-03/) | Event E1 + follow-up + DEL-07 v1→v2 (+ cập nhật BRD) |
| [`sprint-04/`](./sprint-04/) | Event E2 Flash Sale + DEL-08 v1→v2 (+ SRS v2) |
| [`99-verification-checklist.md`](./99-verification-checklist.md) | Checklist kỳ vọng đúng/sai toàn luồng |

## Deliverable bắt buộc theo hệ thống

| Sprint | TemplateCodes |
|--------|----------------|
| 1 | `DEL-02`, `DEL-03` |
| 2 | `DEL-04`, `DEL-05` |
| 3 | `DEL-07` |
| 4 | `DEL-08` |

Stakeholders: **Trần Minh Quân** (Ops), **Lê Hoàng Yến** (CS/Sales), **Nguyễn Quốc Tuấn** (IT).

Events: **E1** nghẽn mạng/trễ đồng bộ tồn (Manual), **E2** Flash Sale Ngày Đôi (Manual).
