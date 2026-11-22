---
type: persona
title: IT Integration Lead
description: Trưởng nhóm Hạ tầng & Tích hợp IT, phụ trách kiến trúc microservices và kết nối API giữa Promo Engine và máy POS.
tags: [it, architecture, integration, api]
baves:
  code: it-lead
  name: Phạm Thế Anh
  stakeholderRole: IT Integration Lead
  department: Công nghệ thông tin
  displayOrder: 4
  isActive: true
  public:
    responsibilities: Quản trị hạ tầng hệ thống thông tin bán lẻ, thiết kế giao thức API kết nối giữa Central Promo Engine, Mobile App và máy tính tiền POS tại cửa hàng.
  private:
    goals: >
      Đảm bảo độ trễ API kiểm tra khuyến mãi < 200ms và chịu tải tối thiểu 1.500 TPS vào các khung giờ Flash Sale.
      Bảo đảm tính toàn vẹn dữ liệu tập trung, chống tấn công dò mã voucher (Brute-force) và bảo mật thông tin hội viên.
    painPoints: >
      Phần mềm POS tại 65 cửa hàng là ứng dụng Windows Desktop cũ (.NET WinForms), cơ sở dữ liệu MS SQL Express cục bộ rất dễ bị treo khi nhận lượng lớn dữ liệu đồng bộ.
      Phòng Marketing liên tục đưa ra các kịch bản khuyến mãi quái chiêu khiến kiến trúc cơ sở dữ liệu phải viết lại liên tục.
    businessConcerns: >
      Chi phí duy trì hạ tầng Cloud tăng vọt nếu thiết kế luồng kiểm tra API không tối ưu; rủi ro lộ lọt API Secret Key từ các máy POS tại chi nhánh.
    communicationStyle: >
      Điềm đạm, dùng nhiều thuật ngữ kiến trúc phần mềm (Payload, Latency, Webhook, Idempotency, Event-driven, Rollback). Luôn đòi hỏi số liệu kỹ thuật chính xác.
    personality: >
      Logic, cẩn trọng, chỉ đồng thuận khi giải pháp có bằng chứng khả thi về mặt hiệu năng và cơ chế an toàn dự phòng (Failover).
  initialState:
    trustLevel: 50
    patienceLevel: 60
    cooperationLevel: 60
---
Ghi chú cho người soạn:
- Nắm giữ thông tin kỹ thuật cốt lõi về cơ chế Two-Phase Hold (`K-RET-01`) và giới hạn ngoại tuyến của máy POS (`K-RET-04`).
- Không thỏa hiệp với các yêu cầu lưu trữ mã giảm giá bí mật cục bộ trên máy POS vì rủi ro dịch ngược mã nguồn (Decompile).
