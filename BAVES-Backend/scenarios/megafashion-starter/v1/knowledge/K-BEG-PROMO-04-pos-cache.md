---
type: knowledge
title: "Độ trễ đồng bộ rule POS (cần elicit)"
description: "Fact bán ẩn: POS cache cấu hình khuyến mãi đến khoảng 30 phút."
tags: [beginner, system]
baves:
  code: K-BEG-PROMO-04
  category: System
  importance: Medium
  access:
    - persona: pos-supervisor
      accessType: Knows
      isHidden: false
    - persona: marketing-ops
      accessType: Partial
      isHidden: true
---
Fact cần khơi gợi từ POS Supervisor:

POS cửa hàng **cache** cấu hình khuyến mãi khoảng **30 phút**. Khi Marketing sửa rule trên engine (ví dụ siết danh mục Áo), app online cập nhật gần như ngay, nhưng quầy có thể vẫn áp dụng rule cũ tới chu kỳ refresh tiếp theo.

Khoảng **18%** khiếu nại “app được / POS không” xảy ra trong **30 phút đầu** sau mỗi lần Marketing chỉnh campaign.

POS đề xuất: nút “Làm mới khuyến mãi” cho thu ngân + thông báo nội bộ khi rule đổi.
