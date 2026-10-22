/** 将一张实物箱标签转换为ASN组盘项。数量来自箱标签，不能用ASN总量代替。 */
export function createAsnBoxItem(
  barcode: string, lines: any[], checks: any[], queued: any[], scanId: string, selectedLineId?: string,
) {
  const fields = barcode.trim().split(',').map(value => value.trim());
  const [materialCode, batchCode, processNo, grade, amount] = fields;
  const quantity = Number(amount);
  if (fields.length < 5 || !materialCode || !Number.isFinite(quantity) || quantity <= 0)
    throw new Error('收料码缺少有效物料或每箱数量');
  const boxNumber = fields.length >= 8 ? fields[7] : '';
  const candidates = lines.filter(line => line.status !== 4 && line.cinvcode === materialCode &&
    (line.cbatch || '') === (batchCode || '') && (!grade || line.cfree5 === grade) &&
    (!selectedLineId || line.autoid === selectedLineId) &&
    Number(line.alreadyStockInQuantity || 0) < Number(line.iquantity));
  if (candidates.length !== 1) throw new Error('箱码与ASN不匹配或来源不唯一，请选择对应ASN明细');
  const line = candidates[0];
  const check = checks.find(row => row.asnLineId === line.autoid);
  if (!check?.canGroup) throw new Error(check?.message || '尚无匹配的合格检验单，请检验完成后重新查询');
  if (boxNumber && queued.some(row => row.asnLineId === line.autoid && row.boxNumber === boxNumber))
    throw new Error('该箱已扫描，不能重复添加');
  const pending = queued.filter(row => row.asnLineId === line.autoid).reduce((sum, row) => sum + Number(row.incellshu), 0);
  if (Number(line.alreadyStockInQuantity || 0) + pending + quantity > Number(line.iquantity))
    throw new Error('本次扫码数量超过ASN剩余待组盘数量');
  const item = {
    materialCode, materialName: line.cinvname || '', goodsSpec: line.cinvstd || '',
    cartonType: line.cfree3 || '',
    goodsBatchNo: batchCode || '', grade: grade || line.cfree5 || '', processNo,
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

