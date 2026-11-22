# 00 — Setup & quy ước demo OmniMart

## Tài khoản & môi trường

| Mục | Giá trị |
|-----|---------|
| Learner | `learner@gmail.com` / `antran596` |
| Kịch bản | `OMNI-001` OmniMart — Cải tiến xử lý đơn hàng đa kênh |
| Free access | Có |
| BYOK | Settings → API Key → Test OK trước khi chat AI |

Backend + FE + agent-runtime phải chạy; DB đã gắn `TemplateCodes` (`DEL-xx`) cho sprint OmniMart.

## Luồng learner chuẩn (không bỏ bước)

```
Login → Ngành/Scenarios → OMNI-001
  → Đọc Context + tab Sprints (thấy DEL-xx)
  → Bắt đầu phiên
  → [Sprint 1] Docs v1 → (phỏng vấn nhẹ) → Nộp → Review Sprint
  → [Sprint 1] Sửa Docs v2 → Nộp lại (trước khi sang sprint 2 hoặc ngay sau review tùy hướng dẫn sprint)
  → [Sprint 2] Phỏng vấn theo Turn → Docs v1 → Nộp → Review
  → [Sprint 2] Docs v2 → Nộp lại
  → [Sprint 3] Trigger/đọc E1 → Follow-up hỏi → Docs → Review
  → [Sprint 4] E2 → Docs RTM → Review → Final Submit → Assessment/Progress
```

## Quy ước version Docs trong bộ demo

| Version | Ý nghĩa | Mục đích kiểm thử |
|---------|---------|-------------------|
| **v1** | Đủ khung, vẫn thiếu mục / số liệu mơ hồ / rule chưa chặt | AI Review nhận ra thiếu sót; Hồ sơ có bản nộp |
| **v2** | Bổ sung chỗ thiếu, siết rule, thêm bằng chứng phỏng vấn | So sánh cải thiện sprint sau; snapshot tốt hơn |

**Cách nộp trên hệ thống:** mỗi lần “Nộp” trên Docs = freeze version + submission. Demo cố ý nộp **v1 trước**, sau đó sửa và nộp **v2**.

## Quy ước phỏng vấn

1. Chỉ hỏi **một Turn một câu** (đúng thứ tự trong file).
2. Đợi agent trả lời xong mới hỏi Turn tiếp.
3. Không gửi cả list câu hỏi trong một tin.
4. Nếu agent né/thiếu số: dùng **Turn follow-up** ngay sau Turn đó (đã ghi trong script), rồi mới sang Turn kế.
5. Có thể bật lưu evidence/bookmark nếu UI có.

## Ánh xạ menu workflow

| Bước | Menu |
|------|------|
| Đọc đề | Scenario detail |
| Chat AI | Phỏng vấn |
| Soạn/nộp DEL | Soạn Docs |
| Xem nộp + review results | Hồ sơ |
| Final | Nghiệm thu / Assessment |

## Bắt đầu demo

1. Login learner.
2. Mở OMNI-001 → xác nhận Sprint 1 hiện `DEL-02`, `DEL-03`.
3. Start session.
4. Sang [`sprint-01/README.md`](./sprint-01/README.md).
