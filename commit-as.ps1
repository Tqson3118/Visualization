# commit-as.ps1 — Commit với danh tính thành viên nhóm (FPT SD21361)
# Cách dùng:  .\commit-as.ps1 son "feat: tao SRS"
#             .\commit-as.ps1 bao "docs: them SDD"
# CẬP NHẬT email bên dưới = email GitHub THẬT của từng người
# (mỗi người vào GitHub -> Settings -> Emails; có thể dùng kiểu <id>+<username>@users.noreply.github.com)

param(
  [Parameter(Mandatory = $true)][string]$Member,
  [Parameter(Mandatory = $true)][string]$Message
)

$members = @{
  son  = @{ Name = "Thai Quang Son";         Email = "thaiquangson@gmail.com" }
  bao  = @{ Name = "Mai Tieu Bao";           Email = "maitieubao@gmail.com" }
  thu  = @{ Name = "Huynh Le Minh Thu";      Email = "thuhlmtd01131@gmail.com" }
  phuc = @{ Name = "Tran Viet Tam Phuc";     Email = "robintran51128@gmail.com" }
}

$key = $Member.ToLower()
if (-not $members.ContainsKey($key)) {
  Write-Host "Khong co thanh vien '$Member'. Dung: $($members.Keys -join ', ')"
  exit 1
}

$m = $members[$key]
Write-Host "Commit voi danh tinh: $($m.Name) <$($m.Email)>"

git -c user.name="$($m.Name)" -c user.email="$($m.Email)" commit -m $Message
if ($LASTEXITCODE -eq 0) { Write-Host "OK. Push: git push" } else { Write-Host "FAIL - xem loi tren" }
