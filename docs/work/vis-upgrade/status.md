# vis-upgrade status

| Task | Mo ta | Trang thai |
|------|-------|------------|
| T6 | Painter enhancements + glow | DONE (c25fb6c) |
| T2 | Array wrap | DONE (80432e3) |
| T4 | LinkedList wrap | DONE (6303812) |
| T5 | Graph meta coords | DONE (ab731d2) |
| T3 | Stack/Queue transition | DONE (0c461ad) |
| T1 | Trace-driven playback | DONE (51f1b39) |

- Full suite: 130/130 PASS (95 cu + 35 moi: T6:+4, T2:+5, T4:+3, T5:+4, T3:+8, T1:+11).
- vue-tsc sach. Build PASS. Bundle engine: 120.59 -> 121.72 kB gzip (+1.13 kB, <5KB).
- Pixel-verify T1: canvas doi theo step (colorSum 31.65M -> 32.35M), khong rong (129k px) — da luu docs/work/vis-upgrade/vis-t1-trace-playback.png.
- Pixel-verify T2: n=60 -> nhieu hang bar (Ollama: 6 hang), index hien thi, khong tran (bounds 42..634/646) — vis-t2-array60-wrap.png.
- Pixel-verify T4: n=15 -> 4-5 hang node, mui ten noi hang, o null cuoi, khong tran (36..614 x 66..454) — vis-t4-linkedlist15-wrap.png.
- Chua lam: dev-review chot + PR base dev + report.
