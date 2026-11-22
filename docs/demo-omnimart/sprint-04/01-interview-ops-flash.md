# Sprint 4 — Ops (Quân) — Event E2 Flash Sale

**Mục đích:** Soft-limit / quota SKU, ưu tiên doanh thu vs kiểm soát tồn, vận hành peaking.

**Quy tắc:** Turn-by-Turn. Không list.

---

### Turn 1 — Bối cảnh campaign
> Anh Quân, flash sale lần này Ops kỳ vọng volume thế nào so với ngày thường? SKU nào “nóng” nhất?

### Turn 2 — Rủi ro tồn
> Với peaking, anh sợ nhất failure mode nào: oversell, hủy hàng loạt, hay lệch tồn kéo dài sau sale?

### Turn 3 — Soft-limit
> Soft-limit theo SKU (hoặc nhóm) anh muốn đặt kiểu gì: % tồn, số đơn trần, hay theo khung giờ? Ai được sửa config lúc sale đang chạy?

### Turn 4 — Tương tác với hold/ATP
> Khi gần chạm soft-limit, hành vi đúng là block confirm, xếp hàng, hay chỉ alert? Hold timeout có cần **rút ngắn** trong cửa sổ flash không?

### Turn 5 — Quan hệ với E1 guards
> Nếu vừa có risk lag sync, flash có được phép “nới” stale-block không, hay **giữ nguyên** guard DEL-07?

### Turn 6 — Ưu tiên Ops cho SRS
> Ba rule Ops cần nhìn thấy trong solution spec (ưu tiên Must): …?

### Turn 7 — Tiêu chí thành công Ops sau flash
> Sau 2 giờ sale + 24h hậu mãi, chỉ số nào bảo Ops là “kiểm soát được”?

---

## Ghi nhanh
- SKU nóng + volume kỳ vọng:
- Soft-limit model:
- Hành vi khi chạm limit:
- Hold/ATP delta lúc flash:
- Giữ / không giữ E1 guard:
