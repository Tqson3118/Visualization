# Study 04 — Code Runner, sandbox và benchmark

> Phạm vi: đọc trực tiếp frontend CodeRunnerView, BenchmarkView/BenchmarkPanel, engines benchmark và backend CodeRuns/CodeRunnerService/validators. Line refs là line hiện tại; chỉ ghi nhận điều có bằng chứng trong source.

## 1. Kết luận

- Code Runner chạy ở client. Store gọi runCode, sau đó chỉ best-effort POST kết quả; backend không thực thi code. Bằng chứng: frontend/src/stores/codeRunner.ts:85-125 và backend/src/DsaVisual.Application/Services/CodeRunnerService.cs:11-14,34-48.
- Sandbox thực tế là interpreter JavaScript có instrumentation trong Web Worker, không phải container/process/OS sandbox. Worker tách UI và có kill switch, nhưng không phải security boundary chống đối thủ.
- Guard có thật: 10.000 bước và 1.000.000 loop ticks (frontend/src/engines/core/stepExecutor.ts:382-387,418-426,824-825); benchmark timeout 5.000 ms/điểm (frontend/src/engines/worker/compileWorker.ts:38-42,101-135; stepExecutor.ts:265-267).
- Không thấy memory quota, heap/RSS/GC/peak allocation hoặc space runtime metric trong execution path. spaceComplexity chỉ là metadata lý thuyết trong codeTemplates.
- Metrics có: durationMs, comparisons, swaps, writes. Benchmark API còn nhận Results do client gửi, lưu chúng và dựng kết luận; không server re-run/attestation.

## 2. Mermaid: luồng thực tế

~~~mermaid
flowchart LR
 A[CodeRunnerView / BenchmarkPanel] --> B[Pinia/API]
 B --> C{Nhánh}
 C -->|Code Runner| D[runCode interpreter]
 C -->|Benchmark| E[runMeasureInWorker]
 D --> F[trace output stats]
 E --> G[Worker measure no trace]
 G --> H{5s deadline/watchdog}
 H -->|timeout/error| I[null / N/A]
 H -->|ok| J[duration counters]
 F --> K[POST code-runs]
 J --> L[POST benchmarks/run]
 K --> M[(CodeRuns)]
 L --> M
 L --> N[Fitted catalog + Conclusion]
~~~

CodeRunnerView gọi codeStore.run tại CodeRunnerView.vue:106-118. BenchmarkPanel chạy từng size rồi từng key tuần tự tại BenchmarkPanel.vue:108-157; đo worker tại dòng 124-139.

## 3. File table

| File | Vai trò và bằng chứng | Giới hạn/gap |
|---|---|---|
| frontend/src/views/CodeRunnerView.vue:106-157 | Run, playback trace, history | trace lỗi/timeout quay về preview; history gọi fetchHistory(0) |
| frontend/src/stores/codeRunner.ts:85-130 | input mặc định, runCode, save | array mặc định [5,3,8,1,9,2,7]; save best-effort |
| frontend/src/engines/core/stepExecutor.ts:254-428 | interpreter, state, counters, guards | measure không sinh frame |
| frontend/src/engines/worker/compileWorker.ts:38-141 | worker lifecycle, watchdog, terminate | một worker cache dùng chung |
| frontend/src/engines/worker/compiler.worker.ts:69-111 | execute measure trong worker | fallbackToRegex=false |
| frontend/src/engines/benchmark/codeTemplates.ts:1-30 | template solve(array), metadata | spaceComplexity là chuỗi lý thuyết |
| frontend/src/components/benchmark/BenchmarkPanel.vue:45-158 | chọn 2–5 key, input, đo, POST | null được map thành 0 trong payload |
| backend/.../Dtos/CodeRunDtos.cs:5-35 | CodeRun request/stats/trace DTO | client gửi status, duration, stats |
| backend/.../Services/CodeRunnerService.cs:21-53 | persist CodeRun | không execute/verify |
| backend/.../Dtos/BenchmarkDtos.cs:6-35 | benchmark request/result/response | Results chứa measurements client |
| backend/.../Validators/BenchmarkRequestValidator.cs:7-25 | validate shape sơ bộ | không whitelist key, bound giá trị size/measurement |
| backend/.../Services/GamificationService.cs:1101-1151 | lưu benchmark, fitted, conclusion | DurationMs=0; tin Results client |
| backend/.../Controllers/GamificationController.cs:224-235 | POST endpoint + validator | [Authorize] ở controller class |
| backend/.../Persistence/Entities/CodeRun.cs:3-16 | DB entity | comment nói GZIP, service serialize JSON thường |

## 4. Sandbox thực tế

### 4.1 Code Runner

1. CodeRunnerView.vue:106-107 gọi codeStore.run.
2. codeRunner.ts:97-100 gọi runCode({ code, entry: 'solve', bindings: [] }, defaultArray).
3. Kết quả thành công lấy trace/stats ở codeRunner.ts:101-108; save ở 111-121.
4. CodeRunnerService.cs:34-48 map request vào CodeRun rồi SaveChangesAsync.

Snippet exact (CodeRunnerService.cs:11-14):

~~~csharp
/// Server KHÔNG chạy code — client sandbox chạy rồi gửi kết quả lên;
/// server chỉ lưu CodeRun (status Success/Error/Timeout, stats, durationMs, traceJson)
/// và trả trace phân trang.
~~~

Controller có [Authorize] (CodeRunsController.cs:9-18); ownership được kiểm tra khi đọc run/trace (CodeRunnerService.cs:56-70,73-96). Đây là authorization dữ liệu, không phải execution isolation.

### 4.2 Benchmark

Benchmark template bọc thân thuật toán thành function solve(a), rồi solve(array) (codeTemplates.ts:26-30). Panel giới hạn UI 2–5 keys (BenchmarkPanel.vue:69-82), chọn sizes từ sizesForComplexity (59-67), tạo random/worst/best input (85-89), và lần lượt gọi runMeasureInWorker (118-141).

Worker gọi:

~~~ts
CompilerStepExecutor.compileAlgorithm(code, toNumberArray(input), {
  fallbackToRegex: false,
  measure: { timeoutMs, counters },
});
~~~

compiler.worker.ts:73-78. Wrapper terminate ở compileWorker.ts:110-115; deadline trong interpreter ở stepExecutor.ts:385-387 và 424-426.

## 5. Limits và security

| Hạng mục | Giá trị/cơ chế | Exact ref |
|---|---|---|
| interpreter steps | 10.000 | stepExecutor.ts:382-384,824 |
| loop ticks | 1.000.000 | stepExecutor.ts:418-426,825 |
| measure timeout | 5.000 ms/điểm | compileWorker.ts:41-42; stepExecutor.ts:265-267 |
| compile watchdog | 15.000 ms | compileWorker.ts:38-42,68-72 |
| kill switch | target.terminate(), worker=null | compileWorker.ts:110-115 |
| API keys | non-empty, count <= 50 | BenchmarkRequestValidator.cs:11-13 |
| API sizes | non-empty, count <= 200 | BenchmarkRequestValidator.cs:15-17 |
| API results | nếu field có thì non-empty; service bắt buộc count > 0 | validator:19-21; GamificationService.cs:1109-1114 |
| language | MaximumLength(20) | validator:23-25 |
| owner read | run.UserId phải bằng current user | CodeRunnerService.cs:65-67,84-86 |

Không thấy trong execution files: Docker/WASM isolation, child process, seccomp/cgroup, CPU quota OS, memory quota, filesystem jail, network egress policy, syscall deny-list, server-side re-execution. Web Worker chỉ là browser execution context + kill switch. Vì vậy không nên gọi đây là sandbox bảo mật cấp OS.

Validator cũng không kiểm tra key tồn tại trong catalog, size dương/upper bound theo giá trị, duplicate, Results khớp Keys×Sizes, N khớp Sizes, duration hữu hạn/không âm, kích thước byte của request/code/output/trace, hoặc language allow-list. Không khẳng định middleware/body limit khác không tồn tại; các gap này là những gì validator được đọc chưa làm.

## 6. Metrics time/space

### Time và counters

Worker bắt đầu performance.now (compiler.worker.ts:73-80), trả durationMs làm tròn cùng comparisons/swaps/writes (79-85). Measure bỏ frame tại stepExecutor.ts:357-359,398-400. Đây là wall-clock của interpreter/instrumentation trong worker, không phải CPU time thuần.

CodeRun DTO có DurationMs và Stats comparisons/swaps/steps (CodeRunDtos.cs:13-25). Nhưng store gửi chỉ comparisons và swaps (codeRunner.ts:117-120); writes/steps không nối đầy đủ vào CodeRun stats.

Không thấy warm-up, nhiều trial, median/p95, confidence interval, seed hoặc environment pinning. Random input có ở Panel:50,85-89. Do đó benchmark wall-clock dễ nhiễu và chưa chứng minh reproducibility.

### Space

Algorithm definition có spaceComplexity và best/worst/average labels (codeTemplates.ts:12-23; ví dụ Bubble:40-43). BenchmarkMeasurementDto chỉ có N, DurationMs, Comparisons, Swaps (BenchmarkDtos.cs:22-28). Không có heap/RSS/GC/peak memory hay memory limit. Không được gọi spaceComplexity là số đo thực nghiệm.

### Server fit/conclusion

GamificationService.cs:1136-1140 tạo Fitted bằng catalogByKey meta.Complexity.Average; đây là lookup nhãn, không regression từ dữ liệu. BuildConclusion ở 1463-1499 chọn measurement tại N lớn nhất, so duration ratio; service lưu OutputJson=request.Results và DurationMs=0 ở 1116-1127. Server không đo round-trip vào duration algorithm.

## 7. Edge cases

1. Code rỗng: codeRunner.ts:86-90 trả lỗi trước engine.
2. Syntax/runtime error: Code Runner error state ở 101-104; benchmark lỗi/worker lỗi resolve null (compileWorker.ts:93-105,117-131).
3. Infinite loop: step/loop guards; benchmark watchdog terminate.
4. Trace lớn: API trace parse JsonDocument và chỉ materialize page (CodeRunnerService.cs:89-132), nhưng save serialize toàn trace (41-44), chưa thấy quota trace.
5. Trace JSON hỏng/non-array: ParseTracePage trả (0,[]) (103-136).
6. Benchmark timeout null: UI hiển thị N/A nhưng buildResults gửi 0 (BenchmarkPanel.vue:91-105), server không phân biệt zero thật và timeout.
7. Key route lạ: BenchmarkView invalid theo catalog (28-34), nhưng API không whitelist; service fallback O(n log n) (GamificationService.cs:1137-1140).
8. API shape: tối đa 50 keys/200 sizes nhưng không thấy giới hạn tương ứng cho từng Results/Measurements.
9. Worker singleton: getWorker cache và mỗi request gán onmessage (compileWorker.ts:44-50,74-82,117-126). UI tuần tự; concurrent callers chưa được chứng minh safe.
10. Backend CodeRun: status parse fail thì mặc định Success (CodeRunnerService.cs:29-32); server không đối chiếu status với error/output/duration.

## 8. Q&A

**Server có chạy/chấm code không?** Không cho Code Runner/benchmark này; server lưu payload client. Exercise flows khác không được đồng nhất nếu chưa đọc chúng.

**Web Worker có phải sandbox an toàn?** Có isolation UI và kill switch, không có bằng chứng memory/filesystem/network/OS isolation hay anti-cheat server.

**Timeout có dừng code không?** Benchmark wrapper terminate worker sau 5s; interpreter deadline cũng check. Đó là giới hạn wall-clock, không phải memory quota.

**Có đo space không?** Không; chỉ metadata Big-O.

**Fitted có fit từ measurements không?** Không; lookup Complexity.Average. Conclusion là heuristic ở N lớn nhất.

**Có thể gửi số liệu giả?** Có thể, vì Results client gửi được lưu và dùng để kết luận; validator không có chữ ký/attestation/server re-run.

**Vì sao UI 5 nhưng API 50?** UX limit và API shape limit độc lập; 5 không phải security quota.

## 9. Gap register

| ID | Gap thực tế | Tác động |
|---|---|---|
| G-01 | Không thấy OS/container/WASM/memory sandbox | Không phải security boundary mạnh |
| G-02 | Không server re-execution/attestation | Client khai báo status/metrics/results |
| G-03 | Không memory/space runtime metric/quota | Không biết peak RAM |
| G-04 | Validator thiếu allow-list key và bound value size/N | Payload/semantics không được khóa đầy đủ |
| G-05 | Không check Results shape; timeout null thành 0 | N/A có thể bị hiểu là 0 |
| G-06 | Chưa thấy CodeRun validator cho code/trace/output/duration trong phạm vi đọc | Có rủi ro storage amplification; cần kiểm tra pipeline chung |
| G-07 | Entity comment GZIP nhưng service serialize JSON trực tiếp | Kỳ vọng storage compression lệch implementation |
| G-08 | Worker singleton handler chưa chứng minh concurrent-safe | Concurrent request có thể tranh handler |
| G-09 | Thiếu warm-up/trials/median/seed/environment | Wall-clock benchmark khó tái lập |
| G-10 | writes/steps không nối nhất quán Code Runner → backend | Metrics contract không đầy đủ |
| G-11 | Benchmark CodeRun DurationMs=0 | History không đại diện tổng thời gian |
| G-12 | Key lạ fallback O(n log n) | Fitted/conclusion có thể misleading |

## 10. Q&A kiểm chứng/nâng cấp

- Nếu mục tiêu là server-grade judging: cần isolated process/container/WASM với CPU, memory, wall-clock, filesystem/network policy; không tin metrics client.
- Cần giới hạn cả count và byte cho code/input/trace/output/measurements.
- Validate allow-list, positive bounded sizes, finite non-negative metrics và shape Results.
- Giữ timeout/N/A khác 0 nếu cần semantics chính xác.
- Thêm warm-up/repetitions/median/p95/seed/environment.
- Đồng bộ writes/steps và xác minh compression thực tế.

> Tóm lại: implementation hiện là client-side instrumented interpreter + Web Worker watchdog + server persistence/heuristic reporting. Đủ cho trải nghiệm học tập/lưu lịch sử, nhưng source không chứng minh sandbox OS-grade, không đo space thực nghiệm và không đảm bảo benchmark trung thực.
