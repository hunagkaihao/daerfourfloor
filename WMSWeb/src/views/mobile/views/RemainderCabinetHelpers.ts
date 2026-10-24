/** 零头柜固定位置，共12柜×15列×8层，按柜、列、层排序。 */
export const remainderCellOptions: { label: string; value: string }[] = [];
for (let cabinet = 0; cabinet < 12; cabinet++) {
  for (let column = 1; column <= 15; column++) {
    for (let layer = 1; layer <= 8; layer++) {
      const code = `${String.fromCharCode(65 + cabinet)}${String(column).padStart(2, '0')}-${String(layer).padStart(2, '0')}`;
      remainderCellOptions.push({ label: code, value: code });
    }
  }
}

/** 识别纯存货码、逗号物料等级卡或实际柜位，不从卡码推断数量。 */
export function parseRemainderScan(value: string) {
  const raw = value.trim();
  const match = raw.toUpperCase().match(/^([A-L])(\d{1,2})-(\d{1,2})$/);
  if (match) {
    const column = Number(match[2]); const layer = Number(match[3]);
    if (column < 1 || column > 15 || layer < 1 || layer > 8) throw new Error('柜位范围为A01-01至L15-08');
    return { cellCode: `${match[1]}${String(column).padStart(2, '0')}-${String(layer).padStart(2, '0')}`, materialCode: '', grade: '' };
  }
  const fields = raw.split(',').map(field => field.trim());
  if (!fields[0] || (fields.length !== 1 && fields.length < 4)) throw new Error('请扫描存货编码、完整物料卡或柜门编码');
  return { cellCode: '', materialCode: fields[0], grade: fields[3] || '' };
}

/** 首次提交生成标识，网络异常后沿用原标识，避免重复加减库存。 */
export function newRemainderRequestId() {
  const browserCrypto = window.crypto as Crypto & { randomUUID?: () => string };
  return browserCrypto?.randomUUID?.() || 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, value => {
    const number = Math.floor(Math.random() * 16);
    return (value === 'x' ? number : (number & 3) | 8).toString(16);
  });
}

/** 显示服务器记录的本地操作时间，不额外转换时区。 */
export function displayRemainderTime(value: string) {
  return value ? value.replace('T', ' ').slice(0, 19) : '-';
}
