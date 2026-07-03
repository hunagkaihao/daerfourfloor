import { withInstall } from '/@/utils';
import expExcelModal from './src/ExportExcelModal.vue';

export { ImpExcel } from './impExcel';
export const ExpExcelModal = withInstall(expExcelModal);
export * from './src/typing';
export { jsonToSheetXlsx, aoaToSheetXlsx } from './src/Export2Excel';
