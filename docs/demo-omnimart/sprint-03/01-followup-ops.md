# Sprint 3 — Follow-up Ops (Quân) — Event E1

**Mục đích:** Xác nhận E1 ảnh hưởng vận hành tồn/đơn thế nào; workaround; ưu tiên rule nào cần siết ngay.

**Quy tắc:** Hỏi **đúng thứ tự Turn**. Không paste list. Đợi trả lời rồi mới Turn sau.

---

### Turn 1 — Xác nhận sự cố
> Anh Quân, em thấy event lệch sync / delay tồn đang chạy. Anh mô tả giúp em **triệu chứng phía Ops** hôm nay: đơn nào đang “nóng”, kho/online đang lệch kiểu gì?

### Turn 2 — Tần suất & mức độ
> Trong ca này, khoảng bao nhiêu đơn / SKU bị ảnh hưởng? Có kho hoặc kênh nào nặng hơn không?

### Turn 3 — Workaround hiện tại
> Team Ops đang xử lý tạm bằng cách nào (hold tay, khóa SKU, chat kho, dừng confirm…)? Cách đó **giữ được bao lâu** và rủi ro gì?

### Turn 4 — ATP / hold dưới sự cố
> Với delay sync, rule ATP gate / hold timeout trong BRD còn đứng được không, hay cần **exception / siết** tạm (ví dụ tăng buffer, giảm soft confirm)?

### Turn 5 — Ưu tiên nghiệp vụ
> Nếu chỉ được 1–2 thay đổi rule **trong sprint này**, anh chọn siết cái gì trước: block confirm khi stale stock, kéo dài hold, hay khóa peaking SKU?

### Turn 6 — Success tạm thời
> Thế nào là “ổn lại” với Ops trong 24–48h tới (chỉ số hoặc điều kiện vận hành)?

### Turn 7 — Open cho IT
> Anh cần IT làm rõ / giao gì để Ops đỡ firefighting? (freshness flag, alert stale, kill-switch…)

---

## Ghi nhanh sau Ops (đưa vào DEL-07)
- Triệu chứng:
- Workaround + rủi ro:
- Rule cần đổi (delta):
- Ưu tiên 1–2:
- Định nghĩa “ổn lại”:
