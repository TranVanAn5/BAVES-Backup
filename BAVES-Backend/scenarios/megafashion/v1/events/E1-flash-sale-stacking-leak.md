---
title: Sự cố rò rỉ mã giảm giá và cộng dồn voucher Flash Sale (Event E1)
description: Lỗi cấu hình chiến dịch Flash Sale nửa đêm cho phép khách hàng cộng dồn đồng thời 2 voucher giảm giá lớn khiến hàng nghìn đơn hàng bị âm giá vốn.
triggerType: Manual
displayOrder: 1
isActive: true
---
Vào lúc 00:00 ngày hội siêu sale giữa tháng, bộ phận Marketing thiết lập nhầm cờ "Cho phép cộng dồn" (Allow Stacking) giữa mã giảm giá 20% toàn sàn và voucher cá nhân 100.000đ dành cho khách VIP.

Kết hợp với độ trễ đồng bộ cache phân tán, hơn 1.200 đơn hàng trực tuyến đã được đặt thành công với mức giảm giá từ 70% đến 95%, trong đó có gần 300 đơn hàng thanh toán thực tế 0 đồng.

Phòng Tài chính đã gửi công văn hỏa tốc yêu cầu dừng toàn bộ luồng đóng gói giao hàng, đồng thời IT Lead và Marketing Lead đang tranh cãi gay gắt về việc tự động hủy đơn hay chịu lỗ để bảo vệ uy tín thương hiệu.
