---
type: knowledge
title: Hao hụt Cân nặng do Thoát ẩm Lạnh lâu ngày (Cold Dehydration)
description: Hiện tượng thực phẩm đông lạnh lưu kho trên 90 ngày bị giảm trọng lượng thực tế và thủ thuật đối phó của đội vận hành kho.
tags: [inventory, weight-loss, audit, hidden]
baves:
  code: K-SCM-04
  category: BusinessRule
  importance: High
  access:
    - persona: dc-director
      accessType: Knows
      isHidden: true
      disclosureRule: Chỉ tiết lộ khi Learner hỏi sâu về nguyên nhân lệch số cân kiểm kê giữa lúc nhập kho và lúc xuất khẩu cho đối tác.
    - persona: qa-lead
      accessType: Partial
      isHidden: false
    - persona: fleet-manager
      accessType: DoesNotKnow
      isHidden: false
    - persona: iot-lead
      accessType: DoesNotKnow
      isHidden: false
---
Đây là nguyên nhân thầm kín gây ra các vụ lệch kiểm kê hàng tháng giữa sổ sách WMS và số cân thực tế bàn giao cho khách hàng xuất khẩu:

1. Cơ chế bốc hơi vi thể trong kho âm sâu (Cold Freezer Burn & Dehydration):
   - Thịt cá và thủy hải sản đông lạnh lưu trữ tại kho -25°C trên 90 ngày sẽ bị thoát ẩm tự nhiên qua màng bao bì xốp, dẫn tới việc hao hụt trọng lượng thực tế từ 1.2% đến 1.8% so với lúc mới nhập kho.
2. Hệ quả tác nghiệp:
   - Một pallet cá hồi ban đầu ghi 1.000 kg, sau 4 tháng thực chất chỉ còn 982 kg.
   - Khi xuất kho theo đơn đặt hàng tính theo kilogram, nhân viên kho thường tự ý lấy thêm các thùng lẻ từ pallet khác bù vào để đủ số cân, dẫn tới tình trạng xuất hiện các "pallet mồ côi" bị thiếu hụt thùng và sai lệch mã số lô hàng (Lot Number) trên hệ thống WMS.
3. Giải pháp khuyến nghị:
   - WMS phải bổ sung trường thông tin `Gross Weight` và `Net Weight` động được cập nhật tại các mốc kiểm kê định kỳ 30 ngày, kèm điều khoản tỷ lệ hao hụt tự nhiên cho phép trong hợp đồng lưu kho 3PL.
