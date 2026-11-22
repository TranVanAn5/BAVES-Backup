---
type: knowledge
title: Ma trận Phân tầng Quyết định Tín dụng (Credit Decision Matrix)
description: Bảng quy tắc phân tầng điểm số tín dụng và hành động tương ứng của hệ thống phê duyệt tự động.
tags: [decision-matrix, credit-scoring, risk, stp]
baves:
  code: K-FIN-02
  category: BusinessRule
  importance: Critical
  access:
    - persona: risk-lead
      accessType: Knows
      isHidden: false
    - persona: biz-director
      accessType: Knows
      isHidden: false
    - persona: fraud-lead
      accessType: Partial
      isHidden: false
    - persona: legal-mgr
      accessType: Partial
      isHidden: false
    - persona: arch-lead
      accessType: DoesNotKnow
      isHidden: false
---
Sau khi tổng hợp điểm số từ mô hình chấm điểm tín dụng FinNova Scorecard (thang điểm từ 300 đến 850), Động cơ Ra quyết định (Decision Engine) thực hiện phân tầng hành động như sau:

1. Hạng A - Khách hàng Rủi ro Cực thấp (Score >= 720):
   - Phê duyệt tự động 100% (STP - Straight-Through Processing) với 100% hạn mức khách hàng yêu cầu (tối đa 70 triệu VNĐ).
   - Thời gian giải ngân tự động: Dưới 3 phút. Không cần thẩm định thủ công, không gọi điện người thân.
2. Hạng B - Khách hàng Rủi ro Thấp (Score từ 650 đến 719):
   - Phê duyệt tự động (STP) nhưng áp dụng quy tắc phòng ngừa rủi ro: Giảm 25% hạn mức so với số tiền yêu cầu (hoặc tối đa 35 triệu VNĐ).
   - Tự động gửi thông báo điều chỉnh hạn mức để khách hàng bấm đồng ý trên App trước khi giải ngân.
3. Hạng C - Khách hàng Ranh giới Cận biên (Score từ 580 đến 649):
   - Chuyển sang luồng Thẩm định Thủ công (Manual Underwriting Queue).
   - Chuyên viên thẩm định gọi điện thoại xác minh 01 số tham chiếu và yêu cầu tải thêm hình ảnh chứng từ thu nhập bổ sung (sao kê ví điện tử, hóa đơn điện/nước). Thời gian xử lý cam kết trong vòng 4 giờ làm việc.
4. Hạng D - Khách hàng Rủi ro Cao (Score < 580):
   - Tự động từ chối (Auto-Reject). Hệ thống hiển thị thông báo lịch sự và hướng dẫn khách cải thiện điểm tín nhiệm sau 30 ngày.
