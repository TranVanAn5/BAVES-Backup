# Sprint 1 — Phỏng vấn theo thứ tự (khảo sát ngữ cảnh)

**Mục đích Sprint 1:** chỉ thu thập đủ để làm rõ *ai quan trọng*, *đau ở đâu*, *nên hỏi sâu gì ở Sprint 2* — **không** đào Business Rules / SRS ở bước này.

**Thứ tự agent:** Ops → CS → IT (đúng kế hoạch trong BAP).

---

## A. Trần Minh Quân — Operations Manager

| Turn | Bạn gửi đúng 1 câu | Mục đích |
|------|-------------------|----------|
| A1 | Xin chào anh Quân, em là BA phụ trách cải tiến xử lý đơn đa kênh OmniMart. Anh có thể mô tả ngắn quy trình hiện tại từ lúc đơn online được xác nhận đến lúc kho bắt đầu soạn hàng không? | Nắm as-is fulfillment |
| A2 | Trong quy trình đó, điểm nào anh thấy lệch tồn hoặc xử lý trùng đơn xảy ra thường xuyên nhất? | Định vị pain Ops |
| A3 | Hiện anh kỳ vọng dự án BA này giúp Ops giảm được vấn đề gì trong 1–2 quý tới? | Goals / success criteria |
| A3-follow *(nếu A3 chung chung)* | Anh có thể nêu 1 chỉ số anh đang theo dõi hàng tuần liên quan hủy đơn hoặc đối soát tồn không? | Ép quantitative nhẹ |

**Dừng Ops.** Chuyển CS.

---

## B. Lê Hoàng Yến — Sales / CS Lead

| Turn | Câu hỏi | Mục đích |
|------|---------|----------|
| B1 | Chào chị Yến, em đang lập kế hoạch BA cho OmniMart. Từ góc CS, khiếu nại nào liên quan đơn hàng / tồn kho đang tốn nhiều thời gian hotline nhất? | Pain CS |
| B2 | Khi website báo còn hàng nhưng kho hết, hiện CS đang trả lời khách và cập nhật hệ thống như thế nào? | Quy trình khiếu nại as-is |
| B3 | Chị muốn thay đổi điều gì trước để giảm khiếu nại kiểu đó? | Expectation / priority |

**Dừng CS.** Chuyển IT.

---

## C. Nguyễn Quốc Tuấn — IT Lead

| Turn | Câu hỏi | Mục đích |
|------|---------|----------|
| C1 | Anh Tuấn ơi, em cần hiểu bức tranh tích hợp hiện tại: POS, WMS và ERP đang đồng bộ tồn/đơn bằng cơ chế gì (batch, API, file…)? | Landscape kỹ thuật |
| C2 | Đâu là ràng buộc kỹ thuật anh không muốn đụng tới khi cải tiến luồng đơn đa kênh? | Constraints / risk |
| C3 | Nếu BA đề xuất “gần realtime”, anh cần thông tin gì từ nghiệp vụ trước khi IT cam kết? | Chuẩn bị câu hỏi Sprint 2 |

**Hết phỏng vấn Sprint 1.** Quay lại Docs làm **v2** rồi **Review Sprint** (theo README Sprint 1).

---

## Ghi chú nhanh sau phỏng vấn (để sửa v2)

- Ops lo: đối soát thủ công, giữ tồn quá lâu.
- CS lo: website ≠ kho thật, thông báo sai lúc.
- IT lo: “realtime” thiếu SLA, không muốn đập hệ thống lõi.
