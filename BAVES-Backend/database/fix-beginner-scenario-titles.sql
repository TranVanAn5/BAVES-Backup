-- Fix Vietnamese titles for beginner scenarios (run with: sqlcmd -f 65001 ...)
UPDATE [scenario].[Scenarios]
SET [Title] = N'OmniMart Starter — Theo dõi trạng thái đơn hàng cơ bản',
    [Description] = N'Beginner: học viên làm quen BA qua bài toán trạng thái đơn hàng online vs cửa hàng, phỏng vấn 1 stakeholder và lập BAP ngắn.'
WHERE [Code] = N'BEG-OMS-01';

UPDATE [scenario].[Scenarios]
SET [Title] = N'AquaCold Starter — Theo dõi nhiệt độ kho lạnh cơ bản',
    [Description] = N'Beginner: làm quen BA trong cold-chain WMS, phỏng vấn giám sát kho về cảnh báo nhiệt độ và nhật ký sự cố đơn giản.'
WHERE [Code] = N'BEG-WMS-01';

UPDATE [scenario].[Scenarios]
SET [Title] = N'FinNova Starter — Làm rõ bước eKYC khoản vay tiêu dùng',
    [Description] = N'Beginner: khảo sát luồng eKYC cơ bản trong Digital LOS, phỏng vấn Credit Ops về điểm nghẽn hồ sơ bị treo.'
WHERE [Code] = N'BEG-LOS-01';

UPDATE [scenario].[Scenarios]
SET [Title] = N'MegaFashion Starter — Làm rõ điều kiện voucher cơ bản',
    [Description] = N'Beginner: khảo sát điều kiện áp dụng voucher đơn giản trong Promotion Engine, phỏng vấn Marketing Ops.'
WHERE [Code] = N'BEG-PROMO-01';
GO
