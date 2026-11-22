---
title: Sự cố cổng API CIC quá tải và Ngân hàng Nhà nước ban hành trần lãi suất mới (Event E2)
description: Cổng tra cứu dữ liệu CIC bị nghẽn làm hàng nghìn hồ sơ bị treo; đồng thời cơ quan quản lý ban hành thông tư mới siết chặt trần lãi suất cho vay tiêu dùng.
triggerType: Manual
displayOrder: 2
isActive: true
---
Vào ngày đầu tiên của tháng, cổng API của Trung tâm Thông tin Tín dụng Quốc gia (CIC) gặp sự cố quá tải đường truyền, tỷ lệ phản hồi lỗi timeout lên tới 75%. Hơn 3.000 hồ sơ vay của khách hàng FinNova bị treo cứng ở bước "Đang tra cứu lịch sử nợ", không thể ra quyết định phê duyệt hoặc từ chối.

Cùng lúc đó, Ngân hàng Nhà nước ban hành thông tư mới có hiệu lực ngay trong tháng sau, quy định mức trần lãi suất cho vay tiêu dùng tín chấp tối đa không được vượt quá 20%/năm và bắt buộc phải giảm tỷ lệ giải ngân tự động nếu không xác thực được cơ sở dữ liệu định danh quốc gia VNeID.

Đội ngũ BA và Kiến trúc sư phải cấp tốc thiết kế lại luồng hàng đợi Circuit Breaker và tính toán lại bảng biểu lãi suất, hạn mức trong Decision Engine để tránh vi phạm pháp luật.
