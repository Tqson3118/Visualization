# 💻 TÀI LIỆU TOÀN TẬP FRONTEND — DSA VISUAL (VUE 3 & PINIA)

Tài liệu này phân tích toàn bộ tầng giao diện và kiến trúc phía máy khách (Frontend) của dự án DSA Visual, xây dựng trên nền tảng **Vue 3 (Composition API / `<script setup>`)**, **TypeScript**, **Pinia State Management**, **Tailwind CSS v4**, **shadcn-vue** và **Visualization Engine độc quyền**.

---

## 🏛️ 1. CẤU TRÚC THƯ MỤC FRONTEND

```
frontend/src/
├── api/                   -> Axios Client, Request/Response Interceptors, API Client Functions
├── components/            -> Reusable UI Components (Shadcn, Base, Simulator, Profile, Studio)
├── composables/           -> Vue Composables (useSimulation, useCodeTracePlayback, useConfetti...)
├── engines/               -> TRÁI TIM KỸ THUẬT: Step Executor (AST Parser), Canvas/Pixi Renderers, Catalog
│   ├── core/              -> stepExecutor.ts (Babel AST Tracer), types.ts, webGpuPipeline.ts
│   ├── generators/        -> 44 Generator Factories (sort, search, tree, graph, linear, hash, heap)
│   ├── renderers/         -> arrayRenderer, treeRenderer, graphRenderer, canvasTheme...
│   └── worker/            -> compileWorker.ts (Web Worker chạy benchmark & sandbox)
├── router/                -> Vue Router 4 (Navigation Guards, RBAC, Scroll Behavior)
├── stores/                -> Pinia Stores (auth, lesson, simulation, gamification, classStore...)
└── views/                 -> 34 View Components (Home, Courses, LessonStudy, Simulator, Studio, Admin...)
```

---

## 📑 DANH SÁCH CÁC TÀI LIỆU CHI TIẾT TRONG THƯ MỤC `study/frontend/`:

| STT | File tài liệu | Nội dung trọng tâm |
|:---:|---|---|
| **1** | [**`01-PINIA-STORES.md`**](file:///d:/FPT/metqua/study/frontend/01-PINIA-STORES.md) | Phân tích chi tiết **9 Pinia Stores**: `auth`, `lesson`, `simulation`, `gamification`, `classStore`, `codeRunner`, `leaderboard`, `progress`, `ui`. |
| **2** | [**`02-AXIOS-API-CLIENTS.md`**](file:///d:/FPT/metqua/study/frontend/02-AXIOS-API-CLIENTS.md) | Phân tích Axios instance, Request Interceptor (tự động gắn JWT), Response Interceptor (tự động Refresh Token), bóc tách các hàm API. |
| **3** | [**`03-VISUALIZATION-ENGINE.md`**](file:///d:/FPT/metqua/study/frontend/03-VISUALIZATION-ENGINE.md) | Đi sâu vào **Babel AST Tracer (`stepExecutor.ts`)**, cơ chế chèn Probe, sinh dòng Trace, Web Worker độc lập và hệ màu Canvas Theme. |
| **4** | [**`04-RENDERERS-AND-GENERATORS.md`**](file:///d:/FPT/metqua/study/frontend/04-RENDERERS-AND-GENERATORS.md) | Phân tích chi tiết các Renderer đồ họa (`arrayRenderer`, `treeRenderer`, `graphRenderer`...) và 44 thuật toán trong `catalog.ts`. |
| **5** | [**`05-COMPOSABLES-AND-ROUTER.md`**](file:///d:/FPT/metqua/study/frontend/05-COMPOSABLES-AND-ROUTER.md) | Phân tích các Composable (`useSimulation`, `useCodeTracePlayback`) và cơ chế Route Guards phân quyền trong `router/index.ts`. |
