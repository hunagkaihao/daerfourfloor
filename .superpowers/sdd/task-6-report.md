# Task 6 Report: ServiceProxy — Manual Addition of New Proxy Methods

## What Was Added

### 1. StockServiceProxy — 3 new methods + process handlers
- `confirmInspectionQualified(stockId, qualifiedQty)` — POST `/wms/stock/confirmInspectionQualified`
- `processConfirmInspectionQualified` — handler returning `ResponseDto`
- `setInspectionNotQualified(stockId)` — POST `/wms/stock/setInspectionNotQualified`
- `processSetInspectionNotQualified` — handler returning `ResponseDto`
- `findByCellAndMaterial(cellCode, materialCode)` — GET `/wms/stock/findByCellAndMaterial`
- `processFindByCellAndMaterial` — handler returning `StockDto`

### 2. ERP_ASNServiceProxy — 1 new method + handler
- `pushCGRKDAdd(input)` — POST `/erp/asn/pushCGRKDAdd`
- `processPushCGRKDAdd` — handler returning `CGRKDAddResponseDto`

### 3. DTO classes (added near end of file, before `FileParameter`)
- `CGRKDAddRequestDto` + `ICGRKDAddRequestDto`
- `CGRKDParams` + `ICGRKDParams`
- `CGRKDAddResponseDto` + `ICGRKDAddResponseDto`

## Files Changed
- `WSMWEB/src/services/ServiceProxies.ts` only

## Insertion Locations (line numbers)

| Item | Line |
|------|------|
| `confirmInspectionQualified` | 21218 |
| `processConfirmInspectionQualified` | 21230 |
| `setInspectionNotQualified` | 21254 |
| `processSetInspectionNotQualified` | 21266 |
| `findByCellAndMaterial` | 21288 |
| `processFindByCellAndMaterial` | 21300 |
| `pushCGRKDAdd` | 4897 |
| `processPushCGRKDAdd` | 4912 |
| `CGRKDAddRequestDto` class | 40294 |
| `ICGRKDAddRequestDto` interface | 40332 |
| `CGRKDParams` class | 40334 |
| `ICGRKDParams` interface | 40346 |
| `CGRKDAddResponseDto` class | 40348 |
| `ICGRKDAddResponseDto` interface | 40377 |

## Verification
- `npx tsc --noEmit --strict false src/services/ServiceProxies.ts` ran successfully for our additions
- All new-symbol errors (`_throw` not found) were fixed by converting catch blocks to the existing codebase pattern
- All remaining TS errors are **pre-existing** (duplicate function implementations from NSwag auto-generation, and module resolution issues unrelated to this change)

## Concerns
- The brief's code used `_throw(_error)` which is not defined in the codebase; fixed by replacing with the existing multi-line catch pattern
- The brief's code style (inline arrow functions, no `cancelToken` in options) differs slightly from the surrounding NSwag-generated style but is functionally equivalent
- NSwag regeneration will overwrite these manual additions; this is expected to be temporary until the backend is running and can regenerate properly
