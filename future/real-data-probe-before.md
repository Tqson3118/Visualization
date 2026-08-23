# Real-data endpoint probe — before Phase A

Baseline build passed with 0 errors. The prior runtime probe was not available in the repository; live authenticated probe will be recorded after the local SQL Server-backed API starts.

Known baseline observations from source audit:
- Profile gamification controllers and simulation catalog are present in source.
- `frontend/vite.real.config.ts` was missing and is recreated.
- SQL Server container `neww-sqlserver-1` was unhealthy because its healthcheck used the old password.
