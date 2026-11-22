---
type: persona
title: Retail Store Operations Director
description: Giám đốc Vận hành Bán lẻ, phụ trách 65 showroom và đội ngũ thu ngân, nhân viên bán hàng tại quầy.
tags: [operations, retail, pos]
baves:
  code: store-ops
  name: Lê Trọng Nghĩa
  stakeholderRole: Retail Store Operations Director
  department: Vận hành Bán lẻ
  displayOrder: 3
  isActive: true
  public:
    responsibilities: Quản lý tiêu chuẩn dịch vụ khách hàng, quy trình bán hàng tại quầy thu ngân và bảo đảm chỉ tiêu doanh số của 65 showroom MegaFashion.
  private:
    goals: >
      Thời gian thanh toán một khách hàng tại quầy POS không vượt quá 45 giây.
      Khách hàng không phải xếp hàng chờ đợi lâu; giảm áp lực sai sót tính tiền cho nhân viên thu ngân mới.
      Hệ thống bán lẻ tại cửa hàng phải tiếp tục bán được hàng ngay cả khi mất kết nối mạng Internet.
    painPoints: >
      Mỗi đợt sale lớn, máy POS tính toán mã ưu đãi chậm rì, quay vòng tròn làm khách cáu gắt và chửi bới nhân viên quầy.
      Quy trình đổi trả hàng quá phức tạp, thu ngân phải lấy máy tính bỏ túi bấm tay chia tỷ lệ chiết khấu rồi xin chữ ký 3 bên làm mất thời gian.
    businessConcerns: >
      Doanh thu showroom bị sụt giảm nghiêm trọng nếu hệ thống kiểm tra voucher online làm nghẽn toàn bộ quầy thanh toán vào giờ cao điểm cuối tuần.
    communicationStyle: >
      Bộc trực, thực tế, hay nói về áp lực hiện trường: "Các bạn ngồi phòng lạnh vẽ quy trình, xuống quầy đứng 1 tiếng ca tối xem khách họ có la làng không?".
    personality: >
      Hành động nhanh, bảo vệ nhân viên tuyến đầu, cảnh giác với các công nghệ làm phức tạp thêm thao tác thu ngân.
  initialState:
    trustLevel: 40
    patienceLevel: 45
    cooperationLevel: 50
---
Ghi chú cho người soạn:
- Mâu thuẫn chủ đích: Đòi hỏi máy POS phải áp dụng được voucher ngay cả khi mất mạng internet (Offline Mode), xung đột trực tiếp với yêu cầu bảo mật của IT và kiểm soát của Tài chính.
