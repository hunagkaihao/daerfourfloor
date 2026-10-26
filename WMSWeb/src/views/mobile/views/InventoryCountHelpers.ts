/** 解析盘点物料码；仅第1段物料必填，等级和产商缺失时留空。 */
export function parseInventoryBarcode(value: string) {
  const raw = value.trim();
  if (!raw || raw.length > 500) throw new Error('请扫描或输入物料码，长度不能超过500个字符');
  const fields = raw.split(',').map(field => field.trim());
  if (!fields[0]) throw new Error('物料编码不能为空');
  const manufacturer = (fields[1] || '').match(/^\d{6}([A-Za-z][A-Za-z0-9]*)$/);
  const manufacturerCode = manufacturer?.[1] || '';
  const grade = fields[3] || '';
  if (fields[0].length > 50 || grade.length > 20 || manufacturerCode.length > 50) {
    throw new Error('物料编码或产商代号不能超过50个字符，等级不能超过20个字符');
  }
  return { materialCode: fields[0], manufacturerCode, grade };
}

/** 允许数量留空并转换为null；已填数量校验整数及范围，三项齐全才计算总数。 */
export function inventoryQuantities(quantityPerBox: unknown, actualBoxes: unknown, remainder: unknown) {
  const values = [quantityPerBox, actualBoxes, remainder].map(value => value === undefined || value === null || value === '' ? null : value);
  if (values.some(value => value !== null && (typeof value !== 'number' || !Number.isInteger(value) || value < 0 || value > 999999999)) || values[0] === 0) {
    throw new Error('已填数量须为整数：整箱数量大于0，实际箱数和零头不小于0，单项最多999999999');
  }
  const [perBox, boxes, loose] = values as (number | null)[];
  const total = perBox !== null && boxes !== null && loose !== null ? perBox * boxes + loose : null;
  if (total !== null && total > 999999999999999) throw new Error('实际总数不能超过999999999999999只');
  return { quantityPerBox: perBox, actualBoxes: boxes, remainderQuantity: loose, totalQuantity: total };
}

/** 数量未填齐或暂不合法时不显示总数，提交时由数量校验给出提示。 */
export function inventoryTotal(quantityPerBox: unknown, actualBoxes: unknown, remainder: unknown): number | null {
  try { return inventoryQuantities(quantityPerBox, actualBoxes, remainder).totalQuantity; } catch { return null; }
}

/** 使用本地年月日作为默认导出日期，避免UTC转换在凌晨落到前一天。 */
export function inventoryToday() {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}
