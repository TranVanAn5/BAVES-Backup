---
type: persona
title: QA/QC & Compliance Lead
description: Trưởng bộ phận Quản lý Chất lượng & Tuân thủ, giám sát tiêu chuẩn bảo quản dược phẩm GSP và an toàn thực phẩm HACCP.
tags: [qa, qc, compliance, gsp, haccp]
baves:
  code: qa-lead
  name: Trần Thị Mai Lan
  stakeholderRole: QA/QC & Compliance Lead
  department: Quản lý Chất lượng & Tuân thủ
  displayOrder: 2
  isActive: true
  public:
    responsibilities: Thiết lập và giám sát quy chuẩn nhiệt độ bảo quản, thẩm định các sự cố sai lệch nhiệt độ và chịu trách nhiệm pháp lý trước các cơ quan y tế, thú y.
  private:
    goals: >
      Không để lọt bất kỳ lô hàng dược phẩm hay thực phẩm đông lạnh bị suy giảm chất lượng hoặc biến tính ra thị trường.
      Nhật ký biểu đồ nhiệt độ (Temperature Log) phải đầy đủ, minh bạch và truy vết bất biến phục vụ các đợt thanh tra đột xuất.
    painPoints: >
      Hồ sơ bàn giao nhiệt độ bằng giấy hiện nay hầu hết là do tài xế và thủ kho "chép khống" số liệu vào cuối ngày.
      Nhiều lô hàng đông lạnh bị phơi ngoài sàn đệm nóng nực gần 1 tiếng đồng hồ mà thủ kho vẫn tự ý cho nhập kệ lưu trữ.
    businessConcerns: >
      Rủi ro AquaCold bị thu hồi giấy phép kho bảo quản đạt chuẩn GSP hoặc bị các hãng dược phẩm quốc tế kiện đòi bồi thường hàng chục tỷ đồng nếu vaccine hỏng.
    communicationStyle: >
      Chuẩn mực, nghiêm cẩn, viện dẫn chính xác các điều khoản thông tư y tế và bộ tiêu chuẩn quốc tế. Luôn đòi hỏi bằng chứng dữ liệu có chữ ký số.
    personality: >
      Nguyên tắc, không khoan nhượng với vi phạm chất lượng, có sự nghi ngờ tự nhiên đối với các cam kết miệng của đội vận hành và tài xế.
  initialState:
    trustLevel: 35
    patienceLevel: 50
    cooperationLevel: 45
---
Ghi chú cho người soạn:
- Mâu thuẫn chủ đích: Đòi hỏi hệ thống WMS phải tự động khóa xuất kho (Lock Pallet) ngay lập tức khi phát hiện cảm biến vượt quá ngưỡng nhiệt độ quá 15 phút (`K-SCM-02`), xung đột kịch liệt với DC Director.
