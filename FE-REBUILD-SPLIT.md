# FE rebuild từ `main` — chia **70% / 30%**

**Tip `origin/main`:** `7237f34`  
**Reference:** `D:\BA-VES-backup\BA-VES`  
**Không** push WIP hiện tại lên GitHub nhóm. Clone `main` sạch → port từ backup → PR nhỏ.

---

## Tổng quan

| Vai trò | % | Trọng tâm |
|---------|---|-----------|
| **FE-30** | ~30% | **Luồng chính: Booking** + vài chức năng nhỏ kèm theo |
| **FE-70** | ~70% | Nền tảng (deps/theme/shell) + hầu hết product còn lại (Docs, Sim, Commerce, Instructor, …) |

FE-70 vẫn làm **nền shell sớm** (PR nền) để cả hai cùng chạy app; FE-30 **không** chỉ làm theme — phần việc chính của họ là Booking E2E.

---

## FE-30 (~30%) — Booking + chức năng nhỏ

### Luồng chính (bắt buộc)

1. **Booking learner**
   - `src/features/booking/BookingPage.tsx`
   - `src/features/booking/BookingChatRoomModal.tsx`
   - `src/services/bookingService.ts`
   - Route `#/booking` + mục sidebar “Đặt lịch”
2. **Booking phía Expert** (tab lịch hẹn trong Expert)
   - Phần tab `bookings` trên Expert (hoặc component tách) dùng chung `bookingService` / chat modal
3. Smoke E2E: login đúng role → mở đặt lịch → tạo/xem lịch → chat phòng (nếu API sẵn)

### Chức năng nhỏ kèm theo (gói trong 30%)

| # | Chức năng | File / phạm vi gợi ý | Vì sao “nhỏ” |
|---|-----------|----------------------|--------------|
| A | **Settings** (profile / đổi mật khẩu / BYOK) | `SettingsModal.tsx`, nối Header, `userService` / `aiService` phần settings | Module khép kín, ít đụng domain BA |
| B | **Learner Cohorts** | `LearnerCohortsPage.tsx`, route `#/cohorts`, item sidebar | 1 page + API list/join |
| C | **Auth login polish** | `LoginPage` + `homeRouteForRoles` / `utils/roles.ts` (phần login) | Cần để vào đúng không gian trước khi book |
| D | **Nav wiring nhẹ** | Chỉ thêm item Booking / Cohorts / nút mở Settings trên Sidebar/Header **đã có sẵn từ FE-70** | Không redesign toàn shell |

### FE-30 **không** làm

- Redesign toàn bộ `globals.css` / Astryx / design system
- Docs TipTap, Simulation/Sprint, Artifacts, Assessment lớn
- Commerce (Cart/Payment/PayOS)
- Instructor authoring / Admin tabs lớn
- Expert approvals / rubrics / standards (chỉ lấy tab **bookings**)

### Definition of done FE-30

- [ ] `#/booking` chạy được (list + tạo/hủy theo đúng API backup)
- [ ] Chat booking mở được khi có phòng
- [ ] Expert vào được tab lịch hẹn (nếu role expert)
- [ ] Settings: sửa profile / password / BYOK cơ bản
- [ ] `#/cohorts` list/join cơ bản
- [ ] Login đưa đúng home theo role (learner/instructor/expert/admin)

---

## FE-70 (~70%) — Nền tảng + product còn lại

### Nền (làm sớm để FE-30 bám vào)

- `package.json` (+ TipTap/Astryx deps)
- `globals.css`, UI primitives (`Button`, `Badge`, …)
- `MainLayout`, **Sidebar/Header khung**, `ScenarioWorkflowBar`
- `AppContext` + `models.ts` (core); FE-30 có thể PR nhỏ bổ sung state booking/settings nếu thiếu
- Skeleton routes (để trống stub cho booking/cohorts — FE-30 thay bằng page thật)

### Product lớn

1. Services còn lại (`apiClient`, simulation, artifact, commerce, catalog, authoring, assessment, …) — **trừ** `bookingService` (FE-30)
2. Dashboard, Applications, Scenario detail/library
3. Simulation + `SprintReviewPanel`
4. Docs (TipTap) + Artifacts + Assessment/Progress
5. Commerce: Cart / Payment / PayOS
6. Instructor (authoring, cohort GV, review, …)
7. Admin + Expert (**trừ** tab bookings do FE-30)

### Definition of done FE-70

- [ ] Shell + theme ổn định, FE-30 chỉ cắm nav/page
- [ ] Learner journey: Applications → Scenario → Sim → Docs → Artifacts → Assessment
- [ ] Cart/Payment chạy được
- [ ] Instructor / Admin / Expert (không tính bookings) đủ từ backup

---

## Thứ tự PR

| # | Owner | Nội dung |
|---|-------|----------|
| 1 | FE-70 | Deps + globals + UI primitives |
| 2 | FE-70 | AppContext + models + layout shell + route stubs |
| 3 | FE-30 | `roles`/Login + Settings gắn Header |
| 4 | FE-30 | **Booking E2E** (service + page + modal + sidebar) |
| 5 | FE-30 | Expert tab bookings + Learner Cohorts |
| 6 | FE-70 | Services còn lại + Learner core (Dashboard/Apps/Scenario/Sim) |
| 7 | FE-70 | Docs + Artifacts + Assessment |
| 8 | FE-70 | Commerce |
| 9 | FE-70 | Instructor + Admin + Expert (rest) |

**Song song:** sau PR2, FE-30 làm PR3–5; FE-70 làm PR6+ trên branch khác, merge thường xuyên.

---

## Va chạm file (thỏa thuận)

| File | Owner chính | FE kia |
|------|-------------|--------|
| `Sidebar.tsx` / `Header.tsx` | FE-70 (khung) | FE-30 chỉ thêm item Booking/Cohorts/Settings |
| `AppRoutes.tsx` / `routes.ts` | FE-70 stub | FE-30 thay stub booking/cohorts |
| `AppContext.tsx` | FE-70 | FE-30 PR nhỏ nếu cần `openSettings` / booking state |
| `ExpertPage.tsx` | FE-70 shell tabs | FE-30 implement tab bookings |

---

## Quy trình backup

1. Clone `main` nhóm → `BA-VES-rebuild`
2. Reference: `D:\BA-VES-backup\BA-VES`
3. Không push từ `D:\BA-VES\BA-VES` lên remote nhóm
4. FE-70 merge PR1–2 trước; FE-30 bám Booking ngay sau đó
