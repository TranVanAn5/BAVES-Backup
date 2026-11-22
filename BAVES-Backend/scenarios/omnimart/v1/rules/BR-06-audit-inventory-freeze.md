---
code: BR-06
title: Điều kiện khóa cập nhật tồn kho khi kiểm kê định kỳ
description: Cửa hàng đang tiến hành kiểm kê thực tế sẽ tạm ngừng đồng bộ tồn bán ra ngoài trong tối đa 2 giờ.
importance: Medium
isHidden: true
---
Quy định nội bộ: Khi cửa hàng bắt đầu ca kiểm kê (stock-take audit), nhân viên quản lý phải kích hoạt cờ 'IsAuditing'. Hệ thống sẽ khóa toàn bộ luồng routing đơn online về chi nhánh này để tránh sai lệch số liệu.
