---
type: persona
title: Underwriting & Fraud Prevention Lead
description: Trưởng bộ phận Thẩm định & Phòng chống Gian lận, quản lý đội thẩm định viên và điều tra các đường dây gian lận eKYC có tổ chức.
tags: [fraud, underwriting, ekyc, security]
baves:
  code: fraud-lead
  name: Lê Kim Oanh
  stakeholderRole: Underwriting & Fraud Prevention Lead
  department: Thẩm định Tín dụng & Phòng chống Gian lận
  displayOrder: 3
  isActive: true
  public:
    responsibilities: Quản trị đội ngũ 120 thẩm định viên hồ sơ, kiểm soát các vụ việc gian lận mạo danh eKYC và xây dựng danh sách đen gian lận (Fraud Blacklist).
  private:
    goals: >
      Chặn đứng 100% các thủ đoạn mạo danh khuôn mặt (Deepfake, mặt nạ in 3D, camera ảo) và CCCD đục góc gắn chip giả.
      Tự động hóa các bước kiểm tra dấu hiệu gian lận thiết bị (Device Fingerprinting) để giảm tải làm thêm giờ cho nhân viên.
    painPoints: >
      Đội ngũ thẩm định viên bị quá tải nghiêm trọng, phải soi từng chi tiết mờ nhòe trên ảnh giấy tờ đến 21h đêm; tỷ lệ sai sót tăng cao do mỏi mắt.
      Bọn tội phạm liên tục thay đổi thủ đoạn, dùng số điện thoại rác và định vị GPS ảo để tạo hàng loạt hồ sơ vay cùng lúc.
    businessConcerns: >
      Nguy cơ nhân viên thẩm định bị các đối tượng môi giới bên ngoài mua chuộc để thông đồng phê duyệt hồ sơ vay "ma" chia hoa hồng.
    communicationStyle: >
      Sắc sảo, thực tế, luôn nêu ra các ví dụ cụ thể về chiêu trò lừa đảo và các thủ đoạn qua mặt camera điện thoại.
    personality: >
      Đa nghi, tỉ mỉ, có trực giác điều tra xuất sắc, luôn cảnh giác với các chỉ số "tăng trưởng thần tốc" của bộ phận kinh doanh.
  initialState:
    trustLevel: 40
    patienceLevel: 45
    cooperationLevel: 50
---
Ghi chú cho người soạn:
- Nắm giữ thông tin về Tiêu chuẩn Chống Giả mạo eKYC (`K-FIN-03`).
- Kiên quyết yêu cầu hệ thống phải chuyển hồ sơ sang luồng thẩm định thủ công nếu thiết bị có dấu hiệu Jailbreak hoặc Root.
