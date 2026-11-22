---
type: knowledge
title: Cơ chế dự phòng khi đứt cáp hoặc mất mạng internet cửa hàng
description: Khi mất mạng, máy POS tự động chuyển sang chế độ offline và lưu giao dịch cục bộ.
tags: [it, pos, network, offline, failover]
baves:
  code: K-IT-03
  category: System
  importance: High
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: false
    - persona: ops-manager
      accessType: Partial
      isHidden: false
    - persona: sales-lead
      accessType: DoesNotKnow
      isHidden: false
---
Nếu cửa hàng mất kết nối internet, máy POS vẫn cho phép nhân viên thu ngân quét mã vạch và thanh toán tiền mặt (offline mode).

Tuy nhiên, toàn bộ giao dịch offline sẽ không trừ tồn kho hệ sinh thái OmniMart ngay lập tức. Khi có mạng trở lại, hàng chục đơn hàng sẽ dồn về cùng lúc (surge sync), dễ gây ra hiện tượng âm kho (negative inventory) nếu trên kênh online cùng lúc đó có khách đặt mua đúng mặt hàng đó.
