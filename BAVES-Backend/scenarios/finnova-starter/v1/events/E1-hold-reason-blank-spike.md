---
title: "Hàng loạt hồ sơ hold thiếu lý do sau đợt cập nhật vendor"
description: "Vendor eKYC đổi mã lỗi nội bộ; LOS chưa map → ~30 hồ sơ vào queue với lý do trống trong buổi sáng."
triggerType: Manual
displayOrder: 1
isActive: true
---
Sau nửa đêm, vendor eKYC đổi một số mã lỗi. Sáng hôm sau khoảng **30 hồ sơ** vào queue Hold với trường lý do **trống**. Ops phải mở log thô; Onboarding nhận case “KYC fail” không rõ.

Chưa có sự cố gian lận; chỉ ảnh hưởng tốc độ xử lý và trải nghiệm khách.

Yêu cầu learner (Beginner): nêu tác động tới SLA và UX Ops, đề xuất **1 rule/AC** cho trường hợp mã vendor chưa map (ví dụ gán VENDOR_TIMEOUT hoặc OTHER + ghi chú), cập nhật ngắn taxonomy — không yêu cầu ứng phó khủng hoảng Intermediate.
