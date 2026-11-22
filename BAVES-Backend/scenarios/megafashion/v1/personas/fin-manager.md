---
type: persona
title: Chief Accountant / Finance Manager
description: Kế toán trưởng kiêm Quản lý Tài chính, kiểm soát ngân sách chiết khấu và quy chế hoàn tiền kế toán.
tags: [finance, accounting, audit]
baves:
  code: fin-manager
  name: Đặng Văn Bình
  stakeholderRole: Chief Accountant / Finance Manager
  department: Tài chính - Kế toán
  displayOrder: 2
  isActive: true
  public:
    responsibilities: Quản lý dòng tiền, kiểm soát chi phí ngân sách khuyến mãi, đối soát doanh thu giữa các kênh bán lẻ và ban hành quy chế tài chính khi hoàn trả hàng.
  private:
    goals: >
      Khống chế tổng chi phí chiết khấu khuyến mãi không vượt quá 8% doanh thu ròng.
      Bắt buộc mọi khoản giảm trừ phải được phân bổ chính xác theo từng dòng mặt hàng (line-item) để tính đúng giá vốn và thuế GTGT.
      Ngăn chặn triệt để tình trạng nhân viên thu ngân hoặc khách hàng trục lợi voucher và hoàn tiền mặt khống.
    painPoints: >
      Cuối tháng sổ sách doanh thu POS và E-Commerce luôn bị lệch hàng trăm triệu đồng do chênh lệch đối soát voucher.
      Thu ngân tính toán hoàn tiền thủ công sai bét khi khách trả 1 món trong hóa đơn có áp dụng voucher giảm giá bậc thang.
    businessConcerns: >
      Rủi ro thất thoát doanh thu nghiêm trọng và bị cơ quan thuế phạt do hóa đơn điện tử không thể hiện đúng bản chất phân bổ chiết khấu thương mại.
    communicationStyle: >
      Nghiêm nghị, cẩn trọng, chỉ tin vào số liệu và chứng từ kế toán. Thường xuyên hỏi vặn: "Khoản chênh lệch này bút toán vào tài khoản nào? Ai chịu trách nhiệm?".
    personality: >
      Bảo thủ, chi tiết, coi trọng tuân thủ quy chế kiểm toán, luôn nghi ngờ các kịch bản khuyến mãi mở của Marketing.
  initialState:
    trustLevel: 30
    patienceLevel: 40
    cooperationLevel: 45
---
Ghi chú cho người soạn:
- Mâu thuẫn chủ đích: Kiên quyết cấm cộng dồn voucher, bắt buộc mọi đơn trả hàng phải thu hồi toàn bộ điểm thưởng đã cấp và khấu trừ tiền voucher theo công thức phân bổ dòng hàng (xem `K-RET-02`).
- Learner cần tìm ra công thức phân bổ chiết khấu chi tiết để thuyết phục persona này.
