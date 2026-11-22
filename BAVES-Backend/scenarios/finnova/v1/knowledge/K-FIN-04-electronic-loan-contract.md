---
type: knowledge
title: Quy định Bắt buộc về Chữ ký số và Hợp đồng Vay Điện tử
description: Các ràng buộc pháp lý bắt buộc theo quy định của Ngân hàng Nhà nước để hợp đồng tín dụng điện tử có hiệu lực khởi kiện.
tags: [legal, contract, otp, e-sign]
baves:
  code: K-FIN-04
  category: Policy
  importance: High
  access:
    - persona: legal-mgr
      accessType: Knows
      isHidden: false
    - persona: biz-director
      accessType: Partial
      isHidden: false
    - persona: risk-lead
      accessType: Partial
      isHidden: false
    - persona: fraud-lead
      accessType: DoesNotKnow
      isHidden: false
    - persona: arch-lead
      accessType: Partial
      isHidden: false
---
Theo quy định tại Luật Giao dịch Điện tử và các Thông tư của Ngân hàng Nhà nước Việt Nam về hoạt động cho vay tiêu dùng số, quy trình ký kết hợp đồng tín dụng trên Mobile App bắt buộc phải đảm bảo các điều kiện sau để có giá trị pháp lý đầy đủ trước Tòa án:

1. Cơ chế hiển thị hợp đồng toàn văn (Full Contract Disclosure):
   - Ứng dụng phải hiển thị toàn bộ văn bản Hợp đồng tín dụng điện tử dưới dạng file PDF có đóng dấu thời gian (Timestamp).
   - Nút bấm "Tôi đồng ý ký hợp đồng" bị làm mờ (Disabled) trong tối thiểu 30 giây đầu tiên, bắt buộc khách hàng phải cuộn đọc qua bảng biểu tính lãi suất, lịch trả nợ gốc-lãi và các khoản phí phạt trước khi có thể bấm nút tiếp tục.
2. Xác thực bằng OTP độc lập (Independent OTP Verification):
   - Mã OTP ký hợp đồng phải là chuỗi 6 chữ số ngẫu nhiên được gửi qua kênh SMS Brandname hoặc Email đã đăng ký chính chủ (không cho phép sử dụng mã OTP tự động điền Autofill mà không có xác nhận của người dùng).
   - Nội dung tin nhắn SMS bắt buộc phải ghi rõ: "Quy khach dang xac nhan ky hop dong vay so tien [Amount] tai FinNova. Ma xac thuc la [OTP]".
3. Lưu trữ chứng cứ điện tử (Audit Trail Integrity):
   - Hệ thống phải lưu trữ toàn bộ bản ghi nhật ký: Địa chỉ IP, Tọa độ GPS thời điểm ký, Mã định danh thiết bị IMEI, Thời điểm nhận OTP và bản hợp đồng PDF có chữ ký số mã hóa SHA-256 trong tối thiểu 10 năm.
