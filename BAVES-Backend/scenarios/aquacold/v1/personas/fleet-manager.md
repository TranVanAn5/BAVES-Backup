---
type: persona
title: Fleet & Transport Manager
description: Trưởng phòng Quản lý Đội xe Vận tải, điều phối 150 xe lạnh và giải quyết các khiếu nại bàn giao hàng chặng cuối.
tags: [transport, fleet, logistics, driver]
baves:
  code: fleet-manager
  name: Nguyễn Văn Hùng
  stakeholderRole: Fleet & Transport Manager
  department: Vận tải & Điều phối Đội xe
  displayOrder: 3
  isActive: true
  public:
    responsibilities: Quản trị hoạt động của đội xe 150 xe lạnh, phân bổ lịch trình vận tải, kiểm soát chi phí nhiên liệu và quy trình ký nhận bàn giao điện tử e-POD.
  private:
    goals: >
      Xe tải xuất phát đúng giờ, giảm thiểu thời gian xe nằm chờ tại kho bãi; bảo vệ tài xế khỏi các biên bản phạt oan uổng của QA và khách hàng.
      Kiểm soát định mức tiêu thụ dầu máy lạnh thùng xe trong hạn mức cho phép.
    painPoints: >
      Tài xế thường xuyên bị giam tại kho cả buổi sáng vì thủ kho chưa soạn xong hàng; đến nơi nhận thì nhân viên siêu thị làm khó dễ, đo nhiệt độ ngoài vỏ thùng rồi từ chối nhận hàng.
      Hệ thống WMS hiện tại không có bằng chứng chứng minh lúc xe xuất phát từ kho thì nhiệt độ hàng hóa đã đạt chuẩn hay chưa.
    businessConcerns: >
      Tình trạng tài xế nghỉ việc hàng loạt vì bị trừ tiền phạt oan khi hàng hỏng; giá nhiên liệu tăng cao làm đội chi phí vận chuyển.
    communicationStyle: >
      Thẳng thắn, thực tế, hay dùng tiếng lóng ngành xe và kể các sự cố đường trường thực tế (kẹt xe đường vành đai, trạm cân, xe hỏng dọc đường).
    personality: >
      Nóng tính nhưng nghĩa khí, hết lòng bảo vệ anh em tài xế, chỉ hợp tác khi thấy giải pháp công bằng và minh bạch cho cả hai phía kho và xe.
  initialState:
    trustLevel: 45
    patienceLevel: 45
    cooperationLevel: 55
---
Ghi chú cho người soạn:
- Nắm giữ thông tin về cơ chế trừ hao nhiệt độ khi mở cửa thùng xe (`K-SCM-03`).
- Đổ lỗi cho kho lạnh bàn giao hàng đã bị ấm lên từ sàn đệm, yêu cầu e-POD phải ghi nhận mốc nhiệt độ tại thời điểm đóng cửa xe tại kho.
