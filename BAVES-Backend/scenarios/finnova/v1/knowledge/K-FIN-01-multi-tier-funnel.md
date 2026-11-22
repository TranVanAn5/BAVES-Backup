---
type: knowledge
title: Kiến trúc Phễu Thẩm định Đa tầng (Multi-Tier Underwriting Funnel)
description: Thiết kế phễu lọc hồ sơ vay nhiều lớp nhằm tối ưu hóa chi phí API tra cứu bên thứ ba và rút ngắn thời gian phê duyệt tự động.
tags: [funnel, underwriting, cost-optimization, architecture]
baves:
  code: K-FIN-01
  category: Process
  importance: Critical
  access:
    - persona: risk-lead
      accessType: Knows
      isHidden: false
    - persona: arch-lead
      accessType: Knows
      isHidden: false
    - persona: biz-director
      accessType: Partial
      isHidden: false
    - persona: fraud-lead
      accessType: Partial
      isHidden: false
    - persona: legal-mgr
      accessType: DoesNotKnow
      isHidden: false
---
Để không lãng phí chi phí tra cứu dữ liệu trả phí (đặc biệt là chi phí truy vấn cổng CIC khoảng 30.000đ - 50.000đ/lượt) và đẩy nhanh tốc độ duyệt đơn, quy trình Digital LOS chia việc thẩm định thành 4 tầng phễu tuần tự (Funnel Tiers):

1. Tầng 1 - Sàng lọc tiêu chí cứng nội bộ (Hard Rules & Internal Blacklist):
   - Kiểm tra ngay trên database nội bộ: Độ tuổi (20-55 tuổi), khu vực hỗ trợ, kiểm tra xem khách có nằm trong danh sách đen nội bộ hoặc đang có nợ quá hạn tại FinNova hay không. Thời gian xử lý < 200ms. Chi phí: 0 đồng.
   - Nếu trượt tầng 1 -> DỪNG NGAY và từ chối hồ sơ, không gọi bất kỳ API nào khác.
2. Tầng 2 - Xác thực định danh & Chống giả mạo eKYC:
   - Quét OCR giấy tờ, kiểm tra Liveness khuôn mặt và so khớp ảnh chân dung với ảnh CCCD. Kiểm tra dấu hiệu giả lập thiết bị di động (Device Fingerprint). Thời gian < 3 giây.
   - Nếu trượt tầng 2 -> DỪNG NGAY và đánh dấu cờ gian lận.
3. Tầng 3 - Tra cứu Dữ liệu Viễn thông & Dữ liệu Thay thế giá rẻ:
   - Gọi API chấm điểm nhà mạng (Telco Score, dữ liệu nạp tiền, tuổi thọ SIM) với chi phí thấp (khoảng 3.000đ/lượt).
   - Nếu điểm viễn thông quá thấp (dưới ngưỡng tối thiểu) -> DỪNG NGAY.
4. Tầng 4 - Tra cứu Lịch sử Tín dụng CIC & Chấm điểm Tín dụng Tổng hợp:
   - Chỉ những hồ sơ đã vượt qua xuất sắc cả 3 tầng trên mới được cấp phép gọi API tra cứu nợ xấu tại CIC và nạp vào Decision Engine để tính toán hạn mức vay cuối cùng.
   - Mô hình này giúp tiết kiệm 42% tổng ngân sách tra cứu dữ liệu hàng tháng cho FinNova.
