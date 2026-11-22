---
type: knowledge
title: Quy tắc Cách ly Lô hàng Vi phạm Nhiệt độ (Automated QC Lock)
description: Cơ chế tự động khóa trạng thái pallet trên hệ thống WMS khi phát hiện cảm biến vượt ngưỡng bảo quản an toàn.
tags: [qc, lock, temperature, wms]
baves:
  code: K-SCM-02
  category: Policy
  importance: Critical
  access:
    - persona: qa-lead
      accessType: Knows
      isHidden: false
    - persona: dc-director
      accessType: Knows
      isHidden: false
    - persona: iot-lead
      accessType: Partial
      isHidden: false
    - persona: fleet-manager
      accessType: DoesNotKnow
      isHidden: false
---
Để đảm bảo tuân thủ tiêu chuẩn thực hành tốt bảo quản thuốc (GSP) và tiêu chuẩn an toàn vệ sinh thực phẩm (HACCP), cơ chế tự động khóa chất lượng (Automated QC Lock) trên phần mềm WMS được cấu hình như sau:

1. Điều kiện kích hoạt khóa tự động:
   - Dữ liệu cảm biến IoT gắn trên pallet hoặc trong buồng kho ghi nhận nhiệt độ vượt ngưỡng an toàn liên tục trong 15 phút (ví dụ: kho dược phẩm > 8°C hoặc kho thịt cá > -15°C).
   - Hoặc cảm biến phát hiện nhiệt độ tăng đột biến trên 5°C trong vòng 5 phút (dấu hiệu rò rỉ nhiệt hoặc hỏng máy nén).
2. Tác động lên hệ thống WMS:
   - Trạng thái của toàn bộ các pallet trong vùng nhiệt độ bị ảnh hưởng lập tức chuyển sang `QC_HOLD`.
   - Mã QR định danh của pallet bị vô hiệu hóa đối với các lệnh lấy hàng (Picking Order). Nếu tài xế xe nâng quét mã pallet bị khóa, màn hình máy quét sẽ báo đỏ `PALLET_LOCKED_BY_QA` và khóa lệnh di chuyển.
3. Thẩm quyền mở khóa:
   - Chỉ có tài khoản của Trưởng bộ phận QA (`qa-lead`) mới có quyền mở khóa trạng thái pallet sau khi lấy mẫu kiểm nghiệm vật lý đạt yêu cầu.
