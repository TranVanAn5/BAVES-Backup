---
type: persona
title: WMS & IoT Systems Engineer
description: Trưởng nhóm Kỹ sư Hệ thống WMS & IoT, phụ trách tích hợp thiết bị quét cầm tay, cảm biến BLE/RFID và hạ tầng truyền tin kho lạnh.
tags: [wms, iot, rfid, bluetooth, telemetry]
baves:
  code: iot-lead
  name: Lê Đăng Khoa
  stakeholderRole: WMS & IoT Systems Engineer
  department: Công nghệ & Tự động hóa
  displayOrder: 4
  isActive: true
  public:
    responsibilities: Quản trị phần mềm WMS, thiết kế mạng gateway thu thập dữ liệu cảm biến nhiệt độ thời gian thực (IoT Telemetry) và duy trì độ ổn định của thiết bị quét mã vạch kho lạnh.
  private:
    goals: >
      Duy trì tính sẵn sàng của hạ tầng IoT và WMS đạt 99.9%; chu kỳ gửi dữ liệu cảm biến nhiệt độ ổn định ở mức 3 phút/lần mà không làm sụt pin cảm biến quá nhanh.
      Xây dựng đường truyền gateway chống nhiễu xuyên qua các vách kho lạnh bọc tôn panel dày.
    painPoints: >
      Thiết bị quét cầm tay (Handheld) của nhân viên kho thường xuyên bị chết nguồn hoặc đơ cảm ứng khi làm việc quá 30 phút trong phòng -25°C.
      Phòng QA đòi hỏi cảm biến phải truyền tin 10 giây/lần, điều này sẽ làm cạn pin cảm biến chỉ sau 2 tháng và làm tắc nghẽn băng thông MQTT Broker.
    businessConcerns: >
      Chi phí thay thế thiết bị cảm biến bị mất hoặc hỏng do va đập càng xe nâng; cơ sở dữ liệu thời gian (Time-series DB) bị quá tải dung lượng.
    communicationStyle: >
      Điềm tĩnh, khoa học, phân tích dựa trên kiến trúc phần cứng và giao thức mạng (MQTT, BLE Beacon, LoRaWAN, IP68, Sleep Mode, Battery Life).
    personality: >
      Cẩn trọng, đam mê công nghệ nhưng thực tế, không chấp nhận các đề xuất kỹ thuật viển vông đi ngược lại quy luật vật lý.
  initialState:
    trustLevel: 50
    patienceLevel: 60
    cooperationLevel: 60
---
Ghi chú cho người soạn:
- Nắm giữ giải pháp kỹ thuật về việc bố trí anten trạm lặp (Repeater) và giới hạn truyền sóng trong hầm lạnh (`K-SCM-05`).
