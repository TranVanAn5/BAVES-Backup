---
type: knowledge
title: Giới hạn lưu lượng và khả năng mở rộng của Database trung tâm
description: Cơ sở dữ liệu ERP trung tâm chỉ đáp ứng được tối đa 350 transactions/giây.
tags: [it, database, performance, bottleneck]
baves:
  code: K-IT-04
  category: System
  importance: Medium
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: true
      disclosureRule: Chỉ chia sẻ khi learner hỏi về năng lực xử lý hoặc lý do không thể cập nhật tồn kho real-time.
    - persona: ops-manager
      accessType: DoesNotKnow
      isHidden: false
    - persona: sales-lead
      accessType: DoesNotKnow
      isHidden: false
---
Cơ sở dữ liệu ERP hiện tại sử dụng công nghệ cũ và đang chịu tải phục vụ đồng thời cho kế toán, kho và 25 chi nhánh.

Ngưỡng chịu tải tối đa là 350 TPS (transactions per second). Nếu nâng tần suất đồng bộ từ POS lên real-time, nguy cơ nghẽn kết nối (connection pool exhaustion) là rất cao, dẫn tới treo toàn bộ hệ thống hóa đơn điện tử.
