---
type: knowledge
title: Tiêu chuẩn Thời gian Phơi nhiệt tại Sàn đệm (Staging Dwell Time)
description: Quy định thời gian tối đa pallet hàng lạnh được phép nằm tại khu vực sàn bốc dỡ trước khi đưa vào kho bảo quản.
tags: [cold-chain, inbound, staging, dwell-time]
baves:
  code: K-SCM-01
  category: Process
  importance: Critical
  access:
    - persona: dc-director
      accessType: Knows
      isHidden: false
    - persona: qa-lead
      accessType: Knows
      isHidden: false
    - persona: fleet-manager
      accessType: Partial
      isHidden: false
    - persona: iot-lead
      accessType: DoesNotKnow
      isHidden: false
---
Quy định nghiệm ngặt về chuỗi lạnh (Cold Chain Standard Operating Procedure) của AquaCold phân loại thời gian phơi nhiệt tối đa tại sàn bốc dỡ (Staging Area - nhiệt độ phòng đệm khoảng 12°C - 15°C) như sau:

1. Hàng đông lạnh âm sâu (-18°C đến -25°C, ví dụ: Thủy hải sản, kem, thịt đông lạnh):
   - Thời gian tối đa kể từ lúc mở cửa thùng container đến khi xe nâng đưa pallet qua cửa rèm khí vào buồng lạnh là 20 phút.
   - Nếu pallet nằm tại sàn đệm quá 20 phút, còi báo động tại cửa Dock sẽ phát âm thanh cảnh báo và đèn tín hiệu chuyển sang màu đỏ.
2. Hàng mát kiểm soát nhiệt độ (2°C đến 8°C, ví dụ: Dược phẩm, sinh phẩm y tế, sữa chua):
   - Thời gian tối đa lưu sàn đệm là 35 phút.
   - Bắt buộc kiểm tra cảm biến nhiệt độ bề mặt thùng bằng súng hồng ngoại trước khi ký nhận biên bản Inbound Slip.
3. Khi pallet vượt quá mốc thời gian quy định trên sàn đệm, hệ thống WMS tự động ghi nhận biên bản bất thường (Exception Log) và yêu cầu chuyên viên QA ký duyệt trước khi được phép nhập vào kệ chính.
