---
type: knowledge
title: "Quy ước không cộng dồn với FREESHIP"
description: "Hành vi hệ thống khi khách nhập hai mã."
tags: [beginner, process]
baves:
  code: K-BEG-PROMO-03
  category: Process
  importance: Medium
  access:
    - persona: marketing-ops
      accessType: Knows
      isHidden: false
    - persona: cx-specialist
      accessType: Knows
      isHidden: false
    - persona: pos-supervisor
      accessType: Partial
      isHidden: false
---
Khi khách nhập **GIẢM50K** trước, mã **FREESHIP** bị từ chối với thông báo chung “Mã không thể kết hợp”.

Khi nhập FREESHIP trước rồi GIẢM50K, app giữ FREESHIP và báo GIẢM50K không hợp lệ — thứ tự nhập làm khách bối rối.

CX muốn thông báo rõ: “GIẢM50K không dùng cùng FREESHIP; hãy chọn 1 mã.” Banner hiện **không** nhắc điều này.
