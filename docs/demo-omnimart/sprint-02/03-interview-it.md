# Sprint 2 — Phỏng vấn IT (Nguyễn Quốc Tuấn) theo thứ tự

**Mục đích:** cơ chế sync, latency thật, ràng buộc không đập core, điều kiện để “gần realtime”, rủi ro bảo mật.

Chọn agent **Nguyễn Quốc Tuấn**.

---

**Turn 1**  
Anh Tuấn, hiện POS cửa hàng, WMS kho và ERP đồng bộ tồn/đơn theo batch, API event, hay cơ chế khác? Chu kỳ gần đúng là bao lâu?

**Turn 2**  
Khi đồng bộ trễ hoặc fail, vận hành đang xử lý thủ công thế nào và IT có audit/log để lần không?

**Turn 3**  
Business hay đòi “realtime”. Anh định nghĩa latency chấp nhận được cho tồn khả dụng và cho trạng thái đơn (ví dụ dưới 1 phút, 5 phút, 15 phút) như thế nào?

**Turn 3-follow** *(nếu né số)*  
Giữa các mức <60s / <5 phút / <15 phút, mức nào IT có thể cân nhắc mà không phải thay core ERP?

**Turn 4**  
Ràng buộc kỹ thuật nào là “không đụng” trong giai đoạn này (schema ERP, POS firmware, …)?

**Turn 5**  
Nếu thêm lớp ATP/order-status service, anh yêu cầu gì về idempotency, phân quyền API và audit để không lỏng bảo mật?

**Turn 6**  
Anh cần business cung cấp quyết định rule nào trước (ưu tiên kênh khi tranh tồn, hold timeout…) thì IT mới thiết kế được?

---

**Ghi findings**
- Cơ chế + chu kỳ sync: …
- Fail handling: …
- Latency chấp nhận: …
- No-touch constraints: …
- Security/audit needs: …
- Rules cần chốt từ business: …

**Xong 3 cuộc → viết Docs Sprint 2.**
