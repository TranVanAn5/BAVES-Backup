---
code: BR-01
title: Thời gian giữ tồn kho tạm thời cho đơn online
description: Đơn hàng thanh toán trực tuyến được giữ tồn tối đa 15 phút để chờ xác nhận từ cổng thanh toán; đơn COD được giữ tối đa 24 giờ để gọi xác nhận.
importance: Critical
isHidden: false
---
Hệ thống phải tự động hủy trạng thái giữ hàng (release inventory hold) nếu sau 15 phút (với đơn thanh toán thẻ/ví) hoặc sau 24 giờ (với đơn COD không liên lạc được) mà đơn chưa được xác nhận hợp lệ.
