# Task 2: StockService — Add inspection confirm qualified/not qualified/find methods

## What I Implemented

1. **StockDto.cs** — Added `InspectionCount` (decimal?) property after `InspectionStatus`
2. **IStockService.cs** — Added 3 interface methods:
   - `ConfirmInspectionQualifiedAsync(Guid stockId, decimal qualifiedQty)`
   - `SetInspectionNotQualifiedAsync(Guid stockId)`
   - `FindByCellAndMaterialAsync(string cellCode, string materialCode)`
3. **StockService.cs** — Added 3 implementation methods following the brief's exact code

## Files Changed

- `WMS/src/TuTa.Wms.Application.Contracts/Stocks/Dtos/StockDto.cs` — One property added
- `WMS/src/TuTa.Wms.Application.Contracts/Stocks/IStockService.cs` — Three method declarations added
- `WMS/src/TuTa.Wms.Application/Stocks/StockService.cs` — Three method implementations added (after `SetInspectionCompletedAsync`, before `PushInspectionReportAsync`)

## Build Verification

```
dotnet build src/TuTa.Wms.Application → 0 errors, 8 warnings (all pre-existing)
```

No new warnings introduced by the changes.

## Self-Review Findings

- **Pattern consistency**: `FindByCellAndMaterialAsync` follows the manual mapping pattern of `GetStockInCellWithBarcodeAsync` (no AutoMapper). Null-conditional operators (`?.`) are used for navigation properties, consistent with defensive coding style.
- **Missing/available references**: `StockInHistory`, `InspectionStatus`, `UnitOfWorkManager`, `UserFriendlyException` — all already available via existing using directives and namespace scope.
- **Edge cases handled**: Null stock, wrong inspection status, `qualifiedQty <= 0`, `qualifiedQty > inspectionCount`, empty cellCode/materialCode — all return appropriate error responses or null.
- **StockInHistory constructor** verified to match parameters (14 positional + named `batchNo`).

## Concerns

None. Implementation matches the brief exactly.
