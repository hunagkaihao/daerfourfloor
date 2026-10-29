/** 将实物箱标签关联到唯一或人工选定的ASN明细；允许反包改变批号，按逐箱数量核对来源总量。 */
export function createAsnBoxItem(
  barcode: string, lines: any[], checks: any[], queued: any[], scanId: string, selectedLineId?: string,
) {
  const fields = barcode.trim().split(',').map(value => value.trim());
  const [materialCode, batchCode, processNo, grade, amount] = fields;
  const quantity = Number(amount);
  if (fields.length < 5 || !materialCode || !Number.isFinite(quantity) || quantity <= 0)
    throw new Error('收料码缺少有效物料或每箱数量');
  const boxNumber = fields.length >= 8 ? fields[7] : '';
  // 反包会改变箱码批号中的日期，来源按物料、等级及明细ID确定，不按箱码批号排除。
  const candidates = lines.filter(line => line.status !== 4 && line.cinvcode === materialCode &&
    (!grade || line.cfree5 === grade) &&
    (!selectedLineId || line.autoid === selectedLineId) &&
    Number(line.alreadyStockInQuantity || 0) < Number(line.iquantity));
  if (!candidates.length) throw new Error('箱码物料、等级或剩余数量与当前ASN明细不匹配，请核对组盘来源');
  if (candidates.length > 1) throw new Error('同物料、等级的ASN来源不唯一，请重新选择检验单来源或核对ASN明细');
  const line = candidates[0];
  const check = checks.find(row => row.asnLineId === line.autoid);
  if (!check?.canGroup) throw new Error(check?.message || '该来源尚未确认检验合格，请核对检验状态后重新查询');
  // 同一种物料的箱贴可完全相同，每次物理扫码生成独立标识，箱号仅作为标签信息展示。
  if (!scanId?.trim()) throw new Error('本次扫码缺少记录标识，请重新扫描');
  if (queued.some(row => row.asnBoxScanId === scanId))
    throw new Error('该扫码记录已添加，不能重复处理同一条记录');
  const pending = queued.filter(row => row.asnLineId === line.autoid).reduce((sum, row) => sum + Number(row.incellshu), 0);
  if (Number(line.alreadyStockInQuantity || 0) + pending + quantity > Number(line.iquantity))
    throw new Error('本次扫码数量超过ASN剩余待组盘数量');
  const item = {
    materialCode, materialName: line.cinvname || '', goodsSpec: line.cinvstd || '',
    cartonType: line.cfree3 || '',
    goodsBatchNo: batchCode || '', grade: grade || line.cfree5 || '', processNo,
    // 原批号仅用于显示追溯，实际库存采用箱码批号，来源由asnLineId关联。
    asnBatchCode: line.cbatch || '',
    quantity, countInOnePkgOrBox: quantity, incellshu: quantity,
    baoshu: 1, sanjianshu: 0, boxNumber: boxNumber || undefined,
    dataCode: barcode.trim(), asnLineId: line.autoid, orderCode: line.cordercode,
    asnBoxScanId: scanId,
  };
  // 先核对本次扫码队列；托盘已有库存由后端在写入前统一校验。
  const pallet = [...queued, item];
  const specs = new Set(pallet.map(row => String(row.goodsSpec || '').trim()
    ? 'spec:' + String(row.goodsSpec).trim() : 'code:' + String(row.materialCode || '').trim()));
  if (specs.size > 2) throw new Error('同一托盘至多放置2种不同规格的物料，请分托组盘');
  if (new Set(pallet.map(row => String(row.grade || '').trim())).size > 1)
    throw new Error('不同等级的物料不允许放置在同一个托盘上，请分托组盘');
  if (new Set(pallet.map(row => String(row.cartonType || '').trim())).size > 1)
    throw new Error('不同纸箱包装不允许放置在同一个托盘上，请分托组盘');
  return item;
}

