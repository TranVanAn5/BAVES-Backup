# BA-VES backup snapshot

Bản sao cục bộ của workspace `D:\BA-VES` (FE + BE + `docs`), **không** gắn remote của repo đang làm việc.

- Nguồn: `D:\BA-VES`
- Bản sao: `D:\BA-VES-backup`
- Đã loại: `node_modules`, `bin`/`obj`, `dist`, `.git` cũ, `.env`
- Repo đang dùng tại `D:\BA-VES` **không bị đổi** remote / file

## Đẩy lên GitHub repo mới (một lần)

1. Tạo repo trống trên GitHub (ví dụ `BA-VES-backup`), **không** tick README.
2. Trong PowerShell:

```powershell
cd D:\BA-VES-backup
git remote add origin https://github.com/<USER>/<REPO>.git
git push -u origin main
```

Nếu chưa có GitHub CLI, cài [gh](https://cli.github.com/) rồi:

```powershell
cd D:\BA-VES-backup
gh auth login
gh repo create BA-VES-backup --private --source=. --remote=origin --push
```
