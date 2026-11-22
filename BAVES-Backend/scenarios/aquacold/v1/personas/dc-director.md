---
type: persona
title: DC Operations Director
description: Giám đốc Vận hành Trung tâm Phân phối, phụ trách thông lượng bốc dỡ kho lạnh và năng suất xe nâng.
tags: [operations, warehouse, cold-chain, logistics]
baves:
  code: dc-director
  name: Hoàng Minh Đức
  stakeholderRole: DC Operations Director
  department: Vận hành Kho vận
  displayOrder: 1
  isActive: true
  public:
    responsibilities: Quản trị hoạt động tiếp nhận, lưu trữ và xuất hàng tại 3 Tổng kho DC; tối ưu hóa năng suất bốc dỡ và chi phí điện lạnh vận hành kho.
  private:
    goals: >
      Tăng thông lượng giải phóng xe tại cửa sàn bốc dỡ (Dock Turnaround Time < 45 phút/xe container).
      Giảm chi phí nhân công bốc xếp và chi phí điện lạnh thông qua việc hạn chế tối đa thời gian mở cửa kho lạnh.
      Loại bỏ tình trạng hàng lưu kho quá hạn bị lãng quên ở các góc khuất của kho âm sâu.
    painPoints: >
      Mỗi lần đội QA/QC chặn một lô hàng lại để đo nhiệt độ thủ công là cả dây chuyền bốc dỡ phía sau bị nghẽn ứ hàng tiếng đồng hồ.
      Chi phí tiền điện duy trì nhiệt độ âm sâu lên tới hơn 1.8 tỷ VNĐ/tháng; cửa Dock mở lâu khiến máy nén lạnh chạy quá tải.
    businessConcerns: >
      Khách hàng 3PL đe dọa phạt hợp đồng nếu kho chuẩn bị hàng trễ khiến xe tải giao muộn cho các chuỗi siêu thị.
    communicationStyle: >
      Quyết đoán, định lượng, nói chuyện bằng chỉ số năng suất (Throughput, Pallet per hour, Turnaround time). Không kiên nhẫn với các quy trình lý thuyết làm chậm tiến độ.
    personality: >
      Thực dụng, chịu áp lực vận hành cường độ cao, ưu tiên giải phóng bãi đậu xe và giải phóng cửa Dock.
  initialState:
    trustLevel: 40
    patienceLevel: 40
    cooperationLevel: 50
---
Ghi chú cho người soạn:
- Mâu thuẫn chủ đích: Muốn đẩy nhanh tiến độ bốc dỡ hàng, có xu hướng cho xe nâng nhập hàng ngay mà không cần chờ kết quả thẩm định nhiệt độ của QA/QC (`qa-lead`).
- Nắm giữ thông tin ẩn về hiện tượng hao hụt trọng lượng do mất nước lạnh (`K-SCM-04`).
