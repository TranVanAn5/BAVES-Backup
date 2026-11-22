# Sprint 4 — Event E2: Manual Flash Sale / peaking

## Mục đích sprint
Khi **E2 Flash Sale** kích hoạt, BA phải:
1. Hỏi Sales/Ops/CS/IT theo thứ tự về soft-limit, oversell risk, CS surge, IT capacity.
2. Chốt quy tắc campaign + NFR peaking.
3. Viết **DEL-08** (SRS / solution requirements) — **v1 thiếu → v2 đầy đủ**.

## Thứ tự làm
1. Đọc mô tả E2 trên UI.
2. `01-interview-ops-flash.md` → `02-interview-cskh-flash.md` → `03-interview-it-flash.md` (Turn-by-Turn).
3. Viết DEL-08 v1 rồi v2: `04-del-08-srs-v1-v2.md`.
4. Review Sprint + đối chiếu snapshot sprint trước nếu UI có.

## Deliverable chính
| Code | File |
|------|------|
| DEL-08 | `04-del-08-srs-v1-v2.md` |

## Input bắt buộc mang theo
- DEL-05 BRD (ATP, soft-limit, status, NFR)
- DEL-07 (stale/lag controls) — flash sale **không được** tắt các guard E1 nếu vẫn cần
