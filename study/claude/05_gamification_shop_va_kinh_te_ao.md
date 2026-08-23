# Study 05 — Gamification, Shop và kinh tế ảo

> Source-grounded study: frontend/backend gamification, shop, premium, VietQR, payment, seed và leaderboard; các gap được đánh dấu theo implementation hiện tại.

## Phạm vi và kết luận

Đã đọc frontend gamification, leaderboard, shop, premium, VietQR và backend entities, controllers, services, seed, payment. Hệ thống có vertical slice cho XP, gems, quest, streak, inventory, premium mock-pay và leaderboard. Source of truth còn phân mảnh giữa service cũ, strategy stateless và controller EF trực tiếp.

Gap lớn: quest progress chưa có runtime increment; ShopController đọc balance rồi ghi spend không atomic; UserInventory thiếu unique UserId+ItemId; premium status tự cộng một tháng từ lúc đọc; mock-pay chưa idempotent; contentRef lệch comment FE; level thresholds khác nhau; leaderboard nhận tab nhưng vẫn xếp toàn bộ Student theo TotalXP.

## Mermaid — dòng giá trị

~~~mermaid
graph TD
  U[User XP level streak premium] --> X[Learning event]
  X --> U
  X --> L[Leaderboard / ladder]
  Q[Quest] --> UQ[UserQuest progress]
  UQ -->|claim| GE[Gem Earn ledger]
  GE --> G[Gems balance]
  G -->|buy| GS[Gem Spend ledger]
  GS --> I[UserInventory]
  P[Premium upgrade] --> O[Order]
  O --> QR[VietQR contentRef]
  QR --> V[Verified payment]
  V --> U
~~~

## 1. Frontend file-by-file

### frontend/src/api/gamification.ts

Endpoint map lines 6–25: /me/gamification, /me/quests, /me/streak, /leaderboard, /shop/items, /shop/buy, /me/inventory, /me/inventory/equip, /premium/status, /premium/upgrade, /premium/mock-pay.

DTO summary lines 29–37:

~~~ts
export interface GamificationSummaryDto {
  xp: number;
  level: number;
  xpIntoLevel: number;
  xpForNextLevel: number;
  levelProgressPct: number;
  gems?: number;
}
~~~

FE nhận progress level từ server, không tự tính. Quest adapter lines 200–214 map progress thành current và reward.gems/xp thành rewardGems/rewardXp; description bị set rỗng. RawQuestDto dùng id number trong khi backend entities dùng Guid: contract drift.

Claim adapter chỉ trả reward delta, bỏ gemsTotal. Store cộng local; retry hoặc tab stale có thể làm UI lệch. Leaderboard adapter map PagedResponse.items thành rows và tìm myRank trong page hiện tại; user ngoài page bị null.

~~~ts
export async function buyItem(itemId: number): Promise<{ gemsLeft: number }> {
  return getData<{ gemsLeft: number }>({ method: 'POST', url: GAMIFICATION_ENDPOINTS.buy, data: { itemId } });
}

export async function equipItem(itemId: number, isEquipped: boolean): Promise<void> {
  await client.put(GAMIFICATION_ENDPOINTS.equip, { itemId, isEquipped });
}
~~~

Equip trả body rỗng nên store phải refetch. Premium mapper chấp nhận isPremium hoặc status active nhưng không tự kiểm tra expiresAt đã qua.

### frontend/src/stores/gamification.ts

State gồm hearts/heartsMax/gems/streakDays/freezeAvailable/xp/level/quests/inventory/achievements/premium. fetchSummary là nguồn XP thật; buyItem nhận gemsLeft server rồi fetch inventory; equipItem luôn refetch.

Exact claim path lines 86–92:

~~~ts
const reward = await gamificationApi.claimQuest(id);
const quest = quests.value.find((q) => q.id === id);
if (quest) quest.claimed = true;
gems.value += reward.gems;
xp.value += reward.xp;
~~~

Đây là delta update, không phải authoritative total. API lỗi thường biến state thành empty/unavailable; không dùng mock fallback.

### frontend/src/stores/leaderboard.ts

Store có tab week/level/class, pagination, noClass và lastClassId. Defensive logic:

~~~ts
const effectiveClassId = tab.value === 'class'
  ? (classId ?? lastClassId.value ?? undefined)
  : undefined;
~~~

setNoClass không gọi API khi user chưa vào lớp để tránh 400. Đây là UI fix; backend vẫn chưa lọc class.

### frontend/src/lib/vietqr.ts

Sinh offline EMVCo TLV, POI static 11, currency 704, CRC16-CCITT poly 0x1021/init 0xffff và độ dài theo byte UTF-8.

~~~ts
const head = [
  tlv('00', '01'), tlv('01', '11'), tlv('52', beneficiary.bankBin),
  tlv('53', '704'), tlv('54', String(amount)), tlv('58', 'VN'),
  tlv('59', beneficiary.name), tlv('60', 'HANOI'),
  tlv('62', tlv('01', 'QRIBFTTA') + tlv('08', content)),
].join('');
const crcBlock = head + '6304';
return crcBlock + getCrc16(crcBlock);
~~~

Gaps: amount chưa validate positive integer/safe range; TLV length hai chữ số nên >99 byte hỏng; content cần giới hạn; bankNumber có trong interface nhưng không xuất hiện trong payload snippet; quét QR không phải confirmation. Static QR chỉ an toàn khi backend đối soát order.

### frontend/src/data/shop_items.json

Catalog dev gồm avatar/frame giá 50–600 gems, khác backend seed 120–1600 gems. Không dùng JSON làm pricing authority; FE nên lấy catalog API.

## 2. Backend entities và persistence

### Quest/UserQuest

Quest.cs lưu QuestKey, Title, Description, Period Daily/Weekly/Monthly, ConditionJson, RewardJson, SortOrder, IsActive. UserQuest.cs lưu Progress, Status NotStarted/InProgress/Completed/Claimed, ClaimedAt.

~~~csharp
public void SetProgress(int progress)
{
    Progress = Math.Max(0, progress);
    if (Status == "NotStarted" && Progress > 0) Status = "InProgress";
}

public void MarkClaimed()
{
    Status = "Claimed";
    ClaimedAt = DateTime.UtcNow;
}
~~~

DbContext unique (UserId, QuestId), không có date/period key. Daily reset bằng insert sẽ conflict; reuse row cần reset boundary.

### ShopItem/UserInventory/GemTransaction

ShopItem có Slot avatar/frame/theme/misc và PriceGems. UserInventory chỉ có Equipped/AcquiredAt; không có Quantity, MaxStack hay expiry. GemTransaction là Earn/Spend ledger:

~~~csharp
Type = type == "Spend" ? "Spend" : "Earn";
ReferenceId = referenceId;
CreatedAt = DateTime.UtcNow;
~~~

Amount âm vẫn lọt qua constructor; type lạ bị ép thành Earn. Cần amount > 0, enum/DB check và idempotency reference.

ApplicationDbContext có unique QuestKey và UserQuest, nhưng UserInventory chưa unique (UserId,ItemId). GemTransaction chỉ index (UserId,CreatedAt).

## 3. Backend services/controllers

### GamificationStrategy.cs

Thresholds: 0, 100, 300, 600, 1000, 1500, 2200, 3000 cho level 1–8. XP event config: QUIZ_COMPLETE 50, MODULE_FINISH 100, STREAK_BONUS 25, ACHIEVEMENT 200.

~~~csharp
public static int CalculateLevel(int totalXp)
{
    for (int i = LevelTable.Length - 1; i >= 0; i--)
        if (totalXp >= LevelTable[i].xpRequired) return LevelTable[i].level;
    return 1;
}
~~~

Đây là config/calculation, không chứng minh event nào cũng award thật.

### Infrastructure/Services/GamificationService.cs

~~~csharp
public async Task AwardXPAsync(Guid userId, int amount, string reason)
{
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null) throw new KeyNotFoundException($"User {userId} not found");
    user.AwardXP(amount);
    user.RecordActivity();
    await _unitOfWork.CommitAsync();
}
~~~

XP và activity cùng commit, nhưng amount chưa validate và không thấy event idempotency. Audit xác nhận exercise submit không cộng XP và không có runtime path tăng UserQuest.Progress.

### ShopController.cs

~~~csharp
var gems = await GemsAsync(uid);
if (gems < item.PriceGems) return BadRequest(new { message = "Không đủ gems." });
_ctx.Set<GemTransaction>().Add(new GemTransaction(uid, item.PriceGems, "Spend", item.Id.ToString()));
_ctx.Set<UserInventory>().Add(new UserInventory(uid, item.Id, equipped: false));
await _ctx.SaveChangesAsync(CancellationToken.None);
~~~

Đây là read-then-write: hai buy đồng thời cùng thấy đủ balance. Mua lại luôn insert inventory; không unique ownership/idempotency.

### PremiumController.cs và payment

Status dùng:

~~~csharp
var expiresAt = user?.IsPremium == true ? DateTime.UtcNow.AddMonths(1) : (DateTime?)null;
~~~

Đây không phải expiry lưu trữ. Upgrade map monthly 199000, quarterly 299000, yearly 499000 và tạo Order. Implementation trả DSV{uid:N}{months}T, còn FE comment yêu cầu DSV{userId}T{months}: mismatch đối soát.

Mock-pay mark completed, set reference, set premium nhưng không gate đã completed; response luôn AddMonths(12); plan lạ âm thầm thành monthly.

### LeaderboardV2Controller.cs

~~~csharp
var users = await _ctx.Users.AsNoTracking()
    .Where(u => u.Role == "Student").ToListAsync();
var ordered = users.OrderByDescending(u => u.TotalXP).ToList();
~~~

Controller nhận tab/classId nhưng không filter. Week không phải weekly XP; class không giới hạn classroom; level dùng threshold khác. Rank global trước pagination. LeaderboardService legacy cache top users sliding 15 giây/absolute 60 giây.

## 4. Seed, ladder và economy

RealDataSeeder tạo 4 daily target 3, 2 weekly target 5 reward 150/100, 1 monthly target 10 reward 500/300. Anchor progress daily đầu 3, daily sau 1, weekly 2, monthly 0.

Shop seed: Aurora Frame 120, Scholar Avatar 180, Ocean Theme 240, Ruby Frame 320, Forest Avatar 400, Sunset Theme 550, Code Star 700, Neon Frame 900, FPT Avatar 1200, Legend Theme 1600. Anchor được seed ownership và Earn/Spend demo transactions.

Seed có badge “Vượt bậc thang — Hoàn thành ladder”, learning paths có node locked/active/passed/final test. Không thấy endpoint ladder độc lập; ladder hiện là learning-path progression, chưa phải economy ledger.

~~~mermaid
flowchart LR
  S[RealDataSeeder] --> Q[Quest definitions]
  S --> I[Shop catalog]
  S --> O[Orders]
  Q --> UQ[UserQuest anchor]
  I --> INV[Inventory anchor]
  I --> GL[Demo gem ledger]
  O --> PR[Premium flag]
~~~

## 5. EXP, gems, streak, hearts và consistency

Strategy/service dùng 8 thresholds tới 3000; LeaderboardV2 dùng 16 thresholds tới 14500. Summary/user level có thể khác leaderboard level.

Gems được tính Earn - Spend bằng cách load toàn bộ ledger. Ưu điểm audit/rebuild; nhược điểm query nặng và thiếu invariant amount/idempotency. Cần atomic debit với điều kiện balance đủ.

Invariant streak/hearts: same-day idempotent; yesterday + freeze tiêu đúng một freeze; timezone rõ; heart không âm; regen không ghi đè enter node; premium expiry hạ max và clamp heart. Audit ghi nhận thiếu concurrency token, race heart regen/streak và không có hosted job downgrade.

### Consistency matrix

| Gap thực tế | Mức | Tác động |
|---|---:|---|
| Quest progress không có runtime increment | Cao | không completed/claim |
| User thiếu RowVersion | Cao | XP/gems/hearts/streak lost update |
| Shop read balance rồi write spend | Cao | double spend/âm balance |
| Inventory thiếu unique user-item | Cao | duplicate ownership |
| Daily key thiếu period/date | Cao | reset khó đúng |
| Premium expiry lazy sai | Cao | entitlement/status drift |
| Mock-pay thiếu idempotency | Cao | replay side effect |
| Level tables phân mảnh | Trung | level lệch |
| Leaderboard tabs chưa filter | Trung | week/class chỉ là label |
| myRank chỉ page hiện tại | Trung | user ngoài page null |
| contentRef mismatch | Cao | reconciliation fail |
| VietQR thiếu validation | Trung | payload invalid/abuse |
| Equip uniqueness chưa enforce | Trung | nhiều item cùng slot |
| FE number vs BE Guid | Trung | mapping bất ổn |

## 6. Mermaid — payment state machine

~~~mermaid
stateDiagram-v2
  [*] --> Pending: upgrade
  Pending --> Completed: verified webhook/mock
  Pending --> Cancelled: cancel/expiry
  Pending --> Failed: reject
  Completed --> Completed: renewal by different order
  Completed --> Refunded: refund if supported
~~~

Client mock-pay không dùng production. Payment thật cần webhook signature, amount/order matching, replay protection, audit event. QR được quét không có nghĩa order completed.

## 7. Q&A

**Q1. EXP cộng ở đâu?** AwardXPAsync tồn tại và có config event, nhưng exercise submit không cộng XP; không giả định mọi activity đều award.

**Q2. Level có một công thức?** Chưa: Strategy/service 8 level, leaderboard 16 threshold.

**Q3. Gems có balance column?** Không thấy; là ledger-derived Earn - Spend.

**Q4. Claim quest idempotent?** Nhánh service audit có Claimed=0 là hướng đúng, nhưng FE chỉ nhận delta; cần route duy nhất và totals.

**Q5. Vì sao FE number nhưng entity Guid?** Contract drift; cần DTO duy nhất, thường string UUID, và test.

**Q6. Mua lại item?** Controller luôn insert inventory; có thể duplicate.

**Q7. Week leaderboard là XP tuần?** Không; controller order TotalXP toàn thời gian.

**Q8. Ladder có API riêng?** Không thấy; hiện gắn learning path/badge.

**Q9. VietQR gọi ngân hàng?** Không, payload sinh offline; backend/provider phải verify.

**Q10. Premium tự downgrade?** Audit ghi không có hosted job; status còn sinh expiry tương lai.

**Q11. Sửa mọi gap một patch?** Không nên; tách payment, economy/concurrency, quest events, contracts, leaderboard semantics.

## 8. File-by-file index

### Frontend

- frontend/src/api/gamification.ts — endpoints, DTO mappings, premium/leaderboard/shop contracts.
- frontend/src/stores/gamification.ts — XP/gems/hearts/streak/quest/inventory/premium state.
- frontend/src/stores/leaderboard.ts — tabs, class EmptyState, pagination.
- frontend/src/lib/vietqr.ts — EMVCo TLV, UTF-8 length, CRC.
- frontend/src/data/shop_items.json — dev presentation catalog.
- frontend/src/lib/vietqr.spec.ts, frontend/src/stores/leaderboard.spec.ts, frontend/src/api/__tests__/gamification.pr30.spec.ts — regression anchors.

### Backend

- backend/src/Domain/Entities/Quest.cs, UserQuest.cs — quest definition/state.
- backend/src/Domain/Entities/ShopItem.cs, UserInventory.cs, GemTransaction.cs — economy entities.
- backend/src/Domain/Strategies/GamificationStrategy.cs — level/badges/event config.
- backend/src/Infrastructure/Services/GamificationService.cs — XP/progress/badges.
- backend/src/Infrastructure/Services/LeaderboardService.cs — cached top/rank legacy path.
- backend/src/WebApi/Controllers/GamificationController.cs — config endpoint.
- backend/src/WebApi/Controllers/ShopController.cs — shop and ledger.
- backend/src/WebApi/Controllers/PremiumController.cs — status/upgrade/mock-pay.
- backend/src/WebApi/Controllers/LeaderboardV2Controller.cs — paged global leaderboard.
- backend/src/Infrastructure/Data/ApplicationDbContext.cs — keys/indexes.
- backend/src/Infrastructure/Data/RealDataSeeder.cs — demo quests/shop/orders.
- backend/src/Infrastructure/Services/PaymentService.cs and payment controllers — parallel payment paths.
- docs/work/backend-audit/findings-biz-gamification.md — 13 verified audit findings.

## 9. Checklist và thứ tự hardening

- [ ] Concurrent buy không âm gems và không duplicate ownership.
- [ ] XP event idempotency và một level policy dùng chung.
- [ ] Quest progress atomic/clamp target; claim retry không nhân reward.
- [ ] Streak same-day/freeze race tests.
- [ ] Premium stored expiry; plan lạ 400; completion replay no-op.
- [ ] contentRef duy nhất; QR validate amount và UTF-8 limits.
- [ ] Leaderboard week/class/level server-side; myRank ngoài page.

Thứ tự: P0 payment correctness; P0 atomic economy; P0 quest/XP events; P1 concurrency/contracts; P1 leaderboard scope; P2 ledger/query/cache performance.

> Chỉ sửa study này; các gap trên là gap thực tế theo code/audit.
