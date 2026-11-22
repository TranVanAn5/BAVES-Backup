---
type: knowledge
title: "Ngưỡng nhiệt độ kho đông hiện hành"
description: "Mục tiêu và ngưỡng Warning/Critical đang dùng tại kho đông."
tags: [beginner, policy]
baves:
  code: K-BEG-WMS-01
  category: Policy
  importance: High
  access:
    - persona: warehouse-supervisor
      accessType: Knows
      isHidden: false
    - persona: qa-assistant
      accessType: Knows
      isHidden: false
    - persona: night-shift-lead
      accessType: Partial
      isHidden: false
---
Kho đông mục tiêu: **-18°C ± 2°C** (dải chấp nhận -20°C đến -16°C).

| Mức | Điều kiện (as-is đang hiểu nội bộ) | Kênh cảnh báo hiện tại |
| --- | --- | --- |
| Info | Dao động trong ±2°C nhưng gần biên | Hiếm khi gửi |
| Warning | Vượt **-15°C** quá **10 phút** | Zalo nhóm — cùng template “Nhiệt độ bất thường” |
| Critical | Vượt **-12°C** hoặc mất điện cụm máy lạnh | Zalo nhóm — cùng template, không phân biệt |

Hiện **mọi mức** (khi gửi) đều dùng một tin Zalo giống nhau, không gắn mức nghiêm trọng.
