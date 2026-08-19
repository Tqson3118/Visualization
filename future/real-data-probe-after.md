# Real-Data Phase A Probe Results (After)

**Generated:** 2026-08-19  
**Status:** 100% PASS (0 unexpected 404/409/5xx)

## Endpoint Probe Results

| # | Role | Name | Method & URL | Status | Result |
|---|---|---|---|---|---|
| 1 | Student | Auth Me | `GET /api/v1/auth/me` | 200 OK | PASS |
| 2 | Student | Progress Me | `GET /api/v1/progress/me` | 200 OK | PASS |
| 3 | Student | Gamification | `GET /api/v1/me/gamification` | 200 OK | PASS |
| 4 | Student | Hearts | `GET /api/v1/me/hearts` | 200 OK | PASS |
| 5 | Student | Streak | `GET /api/v1/me/streak` | 200 OK | PASS |
| 6 | Student | Quests | `GET /api/v1/me/quests` | 200 OK | PASS |
| 7 | Student | Inventory | `GET /api/v1/me/inventory` | 200 OK | PASS |
| 8 | Student | Achievements | `GET /api/v1/achievements` | 200 OK | PASS |
| 9 | Student | Premium Status | `GET /api/v1/premium/status` | 200 OK | PASS |
| 10 | Student | Courses | `GET /api/v1/concepts/courses` | 200 OK | PASS |
| 11 | Student | Simulations Catalog | `GET /api/v1/simulations` | 200 OK | PASS |
| 12 | Student | Simulation Detail | `GET /api/v1/simulations/sort.bubble` | 200 OK | PASS |
| 13 | Student | Classes | `GET /api/v1/classes` | 200 OK | PASS |
| 14 | Student | Leaderboard | `GET /api/v1/leaderboard` | 200 OK | PASS |
| 15 | Student | Exercises | `GET /api/v1/exercises` | 200 OK | PASS |
| 16 | Student | Topics | `GET /api/v1/topics` | 200 OK | PASS |
| 17 | Student | Shop Items | `GET /api/v1/shop/items` | 200 OK | PASS |
| 18 | Student | Favorites | `GET /api/v1/favorites` | 200 OK | PASS |
| 19 | Admin | Admin Stats | `GET /api/v1/admin/stats` | 200 OK | PASS |
| 20 | Admin | Admin Users | `GET /api/v1/users?page=1&pageSize=10` | 200 OK | PASS |
| 21 | Admin | Admin Settings | `GET /api/v1/admin/settings` | 200 OK | PASS |
| 22 | Admin | System Settings | `GET /api/v1/settings` | 200 OK | PASS |

## Phase A Summary
- All required endpoints implemented and responding with 200 OK for their respective authenticated roles.
- `AdminStatsView` role distribution ordering and donut color indexing corrected.
- `Order` creation dates distributed over the past 14 days for realistic 7-day revenue charts.
- `FavoritesController` created to handle favorites persistence.
- Settings controller alias `/admin/settings` verified.
