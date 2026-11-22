---
type: knowledge
title: Tiêu chuẩn Chống Giả mạo eKYC (Anti-Spoofing & Liveness Detection)
description: Yêu cầu kỹ thuật ngăn chặn giả mạo khuôn mặt bằng công nghệ Deepfake, video quay lại và mặt nạ in 3D.
tags: [ekyc, liveness, deepfake, security]
baves:
  code: K-FIN-03
  category: Security
  importance: High
  access:
    - persona: fraud-lead
      accessType: Knows
      isHidden: false
    - persona: arch-lead
      accessType: Knows
      isHidden: false
    - persona: risk-lead
      accessType: Partial
      isHidden: false
    - persona: legal-mgr
      accessType: Partial
      isHidden: false
    - persona: biz-director
      accessType: DoesNotKnow
      isHidden: false
---
Để ngăn chặn các nhóm tội phạm công nghệ cao sử dụng video giả mạo Deepfake và giấy tờ tùy thân giả vượt qua khâu định danh, quy chuẩn eKYC của FinNova bắt buộc áp dụng tiêu chuẩn kép:

1. Cơ chế Kiểm tra Sự sống Hai lớp (Dual Liveness Detection đạt chuẩn quốc tế iBeta Level 2):
   - Lớp thụ động (Passive Liveness): Phân tích phản xạ ánh sáng trên da mặt, phân tích độ sâu vân da (Skin Texture Depth) và quang phổ để phát hiện ngay việc đưa màn hình điện thoại/máy tính bảng hoặc mặt nạ silicon trước camera.
   - Lớp chủ động ngẫu nhiên (Active Liveness): Yêu cầu người vay thực hiện 2 động tác ngẫu nhiên do hệ thống sinh ra theo thời gian thực (ví dụ: quay đầu sang trái 30 độ, chớp mắt 2 lần trong 3 giây) nhằm đánh bại các video quay sẵn.
2. Kiểm tra Dấu vân Thiết bị (Device Fingerprinting):
   - Kiểm tra tính nguyên bản của hệ điều hành: Bắt buộc từ chối ngay lập tức nếu phát hiện ứng dụng đang chạy trên phần mềm giả lập Android (BlueStacks, Nox), thiết bị đã bị can thiệp hệ thống (Root/Jailbreak), hoặc phát hiện luồng video đầu vào xuất phát từ một Camera ảo (Virtual Webcam driver).
3. Độ trùng khớp khuôn mặt (Face Matching Threshold):
   - Tỷ lệ trùng khớp giữa ảnh selfie eKYC và ảnh chân dung trích xuất từ thẻ CCCD gắn chip phải đạt tối thiểu 85.0%. Nếu nằm trong khoảng 75.0% - 84.9%, hồ sơ bắt buộc chuyển thẩm định viên đối soát lại thủ công.
