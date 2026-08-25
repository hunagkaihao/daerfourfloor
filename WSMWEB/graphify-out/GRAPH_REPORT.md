# Graph Report - E:\Tuta\project\daer\daerfourfloor\WSMWEB  (2026-08-24)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 1239 nodes · 1306 edges · 80 communities (73 shown, 7 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 11 edges (avg confidence: 0.58)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `86aed88d`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Community 53|Community 53]]
- [[_COMMUNITY_Community 54|Community 54]]
- [[_COMMUNITY_Community 56|Community 56]]
- [[_COMMUNITY_Community 57|Community 57]]
- [[_COMMUNITY_Community 58|Community 58]]
- [[_COMMUNITY_Community 59|Community 59]]
- [[_COMMUNITY_Community 61|Community 61]]
- [[_COMMUNITY_Community 62|Community 62]]
- [[_COMMUNITY_Community 63|Community 63]]
- [[_COMMUNITY_Community 64|Community 64]]
- [[_COMMUNITY_Community 65|Community 65]]
- [[_COMMUNITY_Community 66|Community 66]]
- [[_COMMUNITY_Community 67|Community 67]]
- [[_COMMUNITY_Community 68|Community 68]]
- [[_COMMUNITY_Community 69|Community 69]]
- [[_COMMUNITY_Community 70|Community 70]]
- [[_COMMUNITY_Community 71|Community 71]]
- [[_COMMUNITY_Community 72|Community 72]]
- [[_COMMUNITY_Community 73|Community 73]]
- [[_COMMUNITY_Community 74|Community 74]]

## God Nodes (most connected - your core abstractions)
1. `scanboxCode()` - 5 edges
2. `data()` - 4 edges
3. `scanboxCode()` - 4 edges
4. `scanboxCode()` - 4 edges
5. `queryStocks()` - 4 edges
6. `normalizeRunStatus()` - 3 edges
7. `createStockTask()` - 3 edges
8. `scangoodsCode()` - 3 edges
9. `data()` - 3 edges
10. `data()` - 3 edges

## Surprising Connections (you probably didn't know these)
- `createTask()` --calls--> `createStockTask()`  [INFERRED]
  src/views/mobile/views/CreateStockTask.vue → src/views/mobile/home/home.vue
- `createOutStockTask()` --calls--> `createStockTask()`  [INFERRED]
  src/views/mobile/views/CreateOutStockTaskSummary.vue → src/views/mobile/home/home.vue
- `setup()` --calls--> `emit`  [INFERRED]
  src/views/warehouse/boxs/EditStorageBox.vue → src/views/warehouse/boxs/CreateStorageBox.vue

## Import Cycles
- None detected.

## Communities (80 total, 7 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.05
Nodes (28): emit, [registerCellForm, { getFieldsValue, validate, resetFields, }], [registerModal, { changeOkLoading, closeModal }], submit(), { t }, emit, [registerCellForm, { getFieldsValue, validate, resetFields }], [registerModal, { changeOkLoading, closeModal }] (+20 more)

### Community 1 - "Community 1"
Cohesion: 0.05
Nodes (31): DataItem, dataSource, editableData, emit, getdata(), good, handleTableChange(), materialCode (+23 more)

### Community 2 - "Community 2"
Cohesion: 0.05
Nodes (29): DataItem, dataSource, editableData, emit, getdata(), good, handleTableChange(), materialCode (+21 more)

### Community 3 - "Community 3"
Cohesion: 0.06
Nodes (25): baoshu, barcodeGet(), boxCode, cell, cellCode, CellSelectModal, columns, createLiftIn() (+17 more)

### Community 4 - "Community 4"
Cohesion: 0.07
Nodes (27): cellStatusLabelMap, getRunStatusLabel(), getRunStatusShort(), getStatusLabel(), normalizeRunStatus(), props, runStatusChipClasses(), runStatusChipTitle() (+19 more)

### Community 5 - "Community 5"
Cohesion: 0.07
Nodes (25): asnCode, asnCodeValidated, asnInputRef, asnOrderColumns, AsnOrderGroup, asnOrderGroups, AsnOrderItem, boxCode (+17 more)

### Community 6 - "Community 6"
Cohesion: 0.06
Nodes (5): activeKey, getUserInfo, recheckcount, userStore, viewStore

### Community 7 - "Community 7"
Cohesion: 0.07
Nodes (26): boxCode, BoxModal, { createConfirm }, getByBarcodeBoxCode(), getUserInfo, good, goodheight, goodsCode (+18 more)

### Community 8 - "Community 8"
Cohesion: 0.07
Nodes (24): boxCode, BoxModal, { createConfirm }, getByBarcodeBoxCode(), getUserInfo, goodheight, goodsCode, GoodsDetail (+16 more)

### Community 9 - "Community 9"
Cohesion: 0.08
Nodes (22): boxCode, { createConfirm }, deleteStock(), diskcancel(), fetchLaneCellStatus(), focus1, focus2, goodheight (+14 more)

### Community 10 - "Community 10"
Cohesion: 0.08
Nodes (21): barcodeGet(), boxCode, cellCode, columns, { createConfirm }, getUserInfo, goodheight, goodsCode (+13 more)

### Community 11 - "Community 11"
Cohesion: 0.08
Nodes (25): batchNo, count, dapdata, data(), dataSource, date, emit, findtype (+17 more)

### Community 12 - "Community 12"
Cohesion: 0.08
Nodes (25): actualOutboundQuantity, allStocks, barcode, barcodeInputRef, canCreate, createOutboundTask(), customRow(), executing (+17 more)

### Community 13 - "Community 13"
Cohesion: 0.07
Nodes (17): emit, [registerModal, { changeOkLoading, closeModal }], [registerStorageBoxForm, { getFieldsValue, validate, resetFields }], submit(), { t }, setup(), { createConfirm }, [register, { openModal }] (+9 more)

### Community 14 - "Community 14"
Cohesion: 0.09
Nodes (22): asnCode, asnDataList, asnInputRef, boxCode, confirmNotQualified(), confirmQualified(), { createConfirm }, deleteStock() (+14 more)

### Community 15 - "Community 15"
Cohesion: 0.08
Nodes (18): findtype, fliter, focus1, goodheight, goodsCode, hiddenCount, outCellCode, outCellInputRef (+10 more)

### Community 16 - "Community 16"
Cohesion: 0.09
Nodes (19): columns, count, dapdata, data(), dataSource, date1, date2, findtype (+11 more)

### Community 17 - "Community 17"
Cohesion: 0.09
Nodes (17): boxCode, BoxSelectModal, CellSelectModal, endcellCode, eventFnboxselect(), getByStock(), goodheight, GoodsDetail (+9 more)

### Community 18 - "Community 18"
Cohesion: 0.09
Nodes (17): columns, data, dataSource, date, emit, getdata(), hiscolumns, innerColumns (+9 more)

### Community 19 - "Community 19"
Cohesion: 0.09
Nodes (15): emit, [registerForm, { getFieldsValue, validate, resetFields }], [registerModal, { changeOkLoading, closeModal }], submit(), { t }, emit, [registerForm, { getFieldsValue, validate, setFieldsValue, resetFields }], [registerModal, { changeOkLoading, closeModal }] (+7 more)

### Community 20 - "Community 20"
Cohesion: 0.10
Nodes (20): canOut, cellCode, cellInputRef, customRow(), displayCellCode, executeOut(), executing, finishCheck() (+12 more)

### Community 21 - "Community 21"
Cohesion: 0.10
Nodes (20): canOut, cellCode, cellInputRef, customRow(), displayCellCode, executeOut(), executing, hasBoxQty (+12 more)

### Community 22 - "Community 22"
Cohesion: 0.09
Nodes (14): { createConfirm }, findtype, fliter, focus1, goodheight, goodsCode, GoodsDetail, [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] (+6 more)

### Community 23 - "Community 23"
Cohesion: 0.10
Nodes (13): /@/components/Excel/src/Export2Excel, { createConfirm }, defaultHeader(), [register, { openModal }], [registerAdjustmentModal, { openModal: openAdjustmentModal }], [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys, setSearchFormValues}], { t }, formData (+5 more)

### Community 24 - "Community 24"
Cohesion: 0.12
Nodes (16): columns, count, dapdata, data(), dataSource, findtype, fliter, handleTableChange() (+8 more)

### Community 25 - "Community 25"
Cohesion: 0.11
Nodes (12): boxCode, cellCode, goodheight, GoodsDetail, Ref1, Ref2, [registerGoodsDetailModal, { openModal: openGoodsDetailModal }], screenHeight (+4 more)

### Community 26 - "Community 26"
Cohesion: 0.12
Nodes (14): DataItem, dataSource, editableData, emit, getdata(), good, handleTableChange(), materialCode (+6 more)

### Community 27 - "Community 27"
Cohesion: 0.12
Nodes (15): canBatchOut, cellCode, cellInputRef, displayCellCode, executeBatchOut(), executing, loading, removeStock() (+7 more)

### Community 28 - "Community 28"
Cohesion: 0.14
Nodes (15): count, dataSource, emit, getdata(), good, handleTableChange(), materialCode, openOut() (+7 more)

### Community 29 - "Community 29"
Cohesion: 0.12
Nodes (12): getIsLock, lockStore, errMsg, { hour, month, minute, meridiem, year, day, week }, loading, lockStore, password, { prefixCls } (+4 more)

### Community 30 - "Community 30"
Cohesion: 0.12
Nodes (15): formData, formRef, { getFormRules }, getShow, loading, { notification, createErrorModal }, { prefixCls }, rememberMe (+7 more)

### Community 31 - "Community 31"
Cohesion: 0.13
Nodes (12): findtype, fliter, focus1, generateSummary(), outCellCode, rowSelection, scancellCode(), screenWidth (+4 more)

### Community 32 - "Community 32"
Cohesion: 0.12
Nodes (6): { createConfirm }, [register, { openModal }], [registerCreateCellModal, { openModal: openCreateCellModal }], [registerImportGoodssModal, { openModal: openImportGoodssModal }], [registerTable, { reload , getRowSelection,getSelectRows ,clearSelectedRowKeys}], { t }

### Community 33 - "Community 33"
Cohesion: 0.13
Nodes (9): canGoBack, canGoForward, currentUrl, iframeUrl, isFullscreen, mobileFrame, navigateToUrl(), router (+1 more)

### Community 34 - "Community 34"
Cohesion: 0.13
Nodes (13): formData, formRef, { getFormRules }, getShow, loading, { notification, createErrorModal }, { prefixCls }, rememberMe (+5 more)

### Community 35 - "Community 35"
Cohesion: 0.15
Nodes (13): barcodeGet(), boxCode, CellSelectModal, clear(), createAgv(), { createConfirm }, goods, newBoxCode (+5 more)

### Community 36 - "Community 36"
Cohesion: 0.16
Nodes (12): columns2, count, data(), dataSource, findtype, fliter, getPagedCheckInItems(), good (+4 more)

### Community 37 - "Community 37"
Cohesion: 0.14
Nodes (6): { createConfirm }, [register, { openModal }], [registerCreateCellModal, { openModal: openCreateCellModal }], [registerEditnoplanModal, { openModal: openEditnoplanModal }], [registerTable, { reload }], { t }

### Community 38 - "Community 38"
Cohesion: 0.15
Nodes (10): { createConfirm }, [registerOutstockModal, { openModal: openOutstockModal }], [registerTable, { getDataSource, reload, getSelectRows, clearSelectedRowKeys }], { t }, emit, formState, handleSubmit(), [register, { closeModal }] (+2 more)

### Community 39 - "Community 39"
Cohesion: 0.15
Nodes (7): columns, executing, items, loading, orderCode, orderData, outboundOrderService

### Community 40 - "Community 40"
Cohesion: 0.17
Nodes (7): { createMessage }, defaultHeader(), getStatusText(), [register, { openModal: openExportModal }], [registerTable, { reload, getForm, setFieldsValue }], route, { t }

### Community 41 - "Community 41"
Cohesion: 0.17
Nodes (8): asnCode, asnDataList, asnInput, asnService, loading, MockAsnItem, saving, showSaveSuccess

### Community 42 - "Community 42"
Cohesion: 0.18
Nodes (7): asnCode, asnDataList, asnInput, asnService, loading, saving, showSaveSuccess

### Community 43 - "Community 43"
Cohesion: 0.24
Nodes (9): columns, handleSearch(), hasQueried, loading, mapStock(), queryForm, queryStocks(), stockList (+1 more)

### Community 44 - "Community 44"
Cohesion: 0.18
Nodes (8): barcodeListService, { createMessage }, modifyLoading, [registerEditForm, { getFieldsValue, setFieldsValue, validate }], [registerEditModal, { openModal: openEditModal, closeModal: closeEditModal }], [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys, getForm}], { t }, Window

### Community 45 - "Community 45"
Cohesion: 0.20
Nodes (7): { createConfirm }, handleRestore(), handleStockRestoreEvent(), [register, { openModal }], [registerRestoreModal, { openModal: openRestoreModal }], [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys}], { t }

### Community 46 - "Community 46"
Cohesion: 0.18
Nodes (7): confirmLoading, data, emit, options, props, [registerModal, { changeOkLoading, closeModal }], textarea01

### Community 47 - "Community 47"
Cohesion: 0.22
Nodes (8): handleUnbind(), inputRef, loading, resolveCellCode(), resolvedCode, resultMessage, resultSuccess, stgBinCode

### Community 48 - "Community 48"
Cohesion: 0.20
Nodes (8): barcode, errorMsg, loading, outboundService, parsed, ParsedBarcode, record, resultMsg

### Community 49 - "Community 49"
Cohesion: 0.20
Nodes (5): columns, items, loading, sortCode, sortData

### Community 50 - "Community 50"
Cohesion: 0.20
Nodes (7): devSchema, infoData, [infoRegister], infoSchema, [register], [registerDev], schema

### Community 51 - "Community 51"
Cohesion: 0.22
Nodes (9): calcHeight(), frameRef, getWrapStyle, { headerHeightRef }, heightRef, hideLoading(), loading, { prefixCls } (+1 more)

### Community 52 - "Community 52"
Cohesion: 0.20
Nodes (8): formData, formRef, { getFormRules }, getShow, { handleBackLogin, getLoginState }, loading, { t }, { validForm }

### Community 53 - "Community 53"
Cohesion: 0.20
Nodes (8): formData, formRef, { getFormRules }, getShow, { handleBackLogin, getLoginState }, loading, { t }, { validForm }

### Community 54 - "Community 54"
Cohesion: 0.22
Nodes (7): formData, formRef, { getFormRules }, getShow, { handleBackLogin, getLoginState }, loading, { t }

### Community 56 - "Community 56"
Cohesion: 0.25
Nodes (6): getFormTitle, { getLoginState }, { t }, getShow, { handleBackLogin, getLoginState }, { t }

### Community 57 - "Community 57"
Cohesion: 0.29
Nodes (5): appStore, permissionStore, { prefixCls }, userId, userStore

### Community 58 - "Community 58"
Cohesion: 0.29
Nodes (4): { createConfirm }, [register, { openModal }], [registerTable, { reload }], { t }

### Community 59 - "Community 59"
Cohesion: 0.29
Nodes (4): { createConfirm, message }, [register, { openModal }], [registerTable, { reload, getDataSource }], { t }

### Community 61 - "Community 61"
Cohesion: 0.33
Nodes (3): { createConfirm }, [registerTable, { reload }], { t }

### Community 62 - "Community 62"
Cohesion: 0.33
Nodes (5): globSetting, localeStore, { prefixCls }, { t }, title

### Community 63 - "Community 63"
Cohesion: 0.33
Nodes (5): globSetting, localeStore, { prefixCls }, { t }, title

### Community 64 - "Community 64"
Cohesion: 0.33
Nodes (4): { createConfirm, message }, [register, { openModal }], [registerTable, { reload, getSelectRows }], { t }

### Community 65 - "Community 65"
Cohesion: 0.33
Nodes (4): { createConfirm }, itemColumns, [registerTable, { getDataSource, reload, getSelectRows, clearSelectedRowKeys }], { t }

### Community 66 - "Community 66"
Cohesion: 0.40
Nodes (3): { createConfirm }, getUserInfo, userStore

### Community 67 - "Community 67"
Cohesion: 0.40
Nodes (4): { createError }, modifiedSearchFormSchema, [registerTable, { reload, getForm }], { t }

## Knowledge Gaps
- **704 isolated node(s):** `{ getAntdLocale }`, `userStore`, `{ t }`, `{ createConfirm }`, `[registerTable, { reload }]` (+699 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `createStockTask()` connect `Community 74` to `Community 4`, `Community 6`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **Why does `createTask()` connect `Community 4` to `Community 74`?**
  _High betweenness centrality (0.018) - this node is a cross-community bridge._
- **Why does `createOutStockTask()` connect `Community 74` to `Community 31`?**
  _High betweenness centrality (0.015) - this node is a cross-community bridge._
- **What connects `{ getAntdLocale }`, `userStore`, `{ t }` to the rest of the system?**
  _704 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.04625346901017576 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.0545876887340302 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.054878048780487805 - nodes in this community are weakly interconnected._