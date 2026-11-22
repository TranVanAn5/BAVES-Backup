---
title: "Bỏ lỡ một cảnh báo Warning lúc 02:10 ca đêm"
description: "Một Warning nhiệt độ -14,5°C kéo dài 18 phút lúc 02:10 chỉ được phát hiện khi đi tuần lúc 02:40; chưa thành Critical."
triggerType: Manual
displayOrder: 1
isActive: true
---
Lúc **02:10**, cảm biến ghi **-14,5°C** và giữ trên -15°C hơn 10 phút → Warning gửi Zalo. Ca đêm đang để nhóm im lặng nên không thấy tin.

Lúc **02:40** người trực đi tuần, mở cửa kho và thấy dashboard vàng. Nhiệt về **-16,8°C** sau khi đóng kín cửa và giảm tần suất mở cổng. Không có hư hàng ghi nhận, nhưng QA yêu cầu giải trình.

Yêu cầu learner (Beginner): phân tích tác động nhẹ tới quy trình acknowledge, nêu **gap** as-is, và đề xuất **1–2** chỉnh sửa cho user story cảnh báo/nhật ký — không viết crisis playbook đầy đủ.
