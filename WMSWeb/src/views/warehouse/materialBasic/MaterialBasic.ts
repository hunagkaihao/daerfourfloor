import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import {
  MaterialServiceProxy,
  PagedMaterialsQueryDto,
  MaterialCreateDto,
  MaterialUpdateDto,
  MaterialDto,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({ tip: 'Loading...' });
const _materialServiceProxy = new MaterialServiceProxy();

export const tableColumns: BasicColumn[] = [
  { title: t('物料编码'), dataIndex: 'materialCode' },
  { title: t('物料名称'), dataIndex: 'materialName' },
  { title: t('规格'), dataIndex: 'specs' },
  { title: t('单位'), dataIndex: 'unit' },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编码'),
    component: 'Input',
    colProps: { span: 6 },
    componentProps: {
      placeholder: '支持物料码前缀模糊查询',
      allowClear: true,
    },
  },
  {
    field: 'materialName',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'specs',
    label: t('规格'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'unit',
    label: t('单位'),
    component: 'Input',
    colProps: { span: 6 },
  },
];

export const createFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编码'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
  {
    field: 'materialName',
    label: t('物料名称'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
  {
    field: 'specs',
    label: t('规格'),
    component: 'Input',
    colProps: { span: 24 },
  },
  {
    field: 'unit',
    label: t('单位'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
];

export const editFormSchema: FormSchema[] = [
  {
    field: 'materialCodeNew',
    label: t('物料编码'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
  {
    field: 'materialNameNew',
    label: t('物料名称'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
  {
    field: 'specsNew',
    label: t('规格'),
    component: 'Input',
    colProps: { span: 24 },
  },
  {
    field: 'unitNew',
    label: t('单位'),
    component: 'Input',
    required: true,
    colProps: { span: 24 },
  },
];

function trimValue(value?: string) {
  return value?.trim() || undefined;
}

function resolveTypeCode(materialCode?: string, fallbackTypeCode?: string) {
  if (fallbackTypeCode?.trim()) {
    return fallbackTypeCode.trim();
  }
  const code = materialCode?.trim();
  return code ? code.substring(0, 1) : '0';
}

function getErrorMessage(error: unknown, fallback: string) {
  const err = error as {
    response?: { data?: { error?: { message?: string }; message?: string } };
    message?: string;
  };
  return (
    err?.response?.data?.error?.message ||
    err?.response?.data?.message ||
    err?.message ||
    fallback
  );
}

function filterMaterialList(list: MaterialDto[], params: Record<string, any>) {
  return (list || []).filter((item) => {
    if (params.materialName && item.materialName !== params.materialName) return false;
    if (params.specs && item.specs !== params.specs) return false;
    if (params.unit && item.unit !== params.unit) return false;
    return true;
  });
}

function paginateList<T>(list: T[], pageIndex: number, pageSize: number) {
  const start = (pageIndex - 1) * pageSize;
  return {
    items: list.slice(start, start + pageSize),
    totalCount: list.length,
  };
}

export async function getMaterialListAsync(params: Record<string, any>) {
  openFullLoading();
  try {
    const pageIndex = params.pageIndex || 1;
    const pageSize = params.pageSize || 10;

    if (params.materialCode?.trim()) {
      const fuzzyList = await _materialServiceProxy.materialsWithCodeTipGet(params.materialCode.trim());
      const filtered = filterMaterialList(fuzzyList, params);
      return paginateList(filtered, pageIndex, pageSize);
    }

    const query = new PagedMaterialsQueryDto();
    query.materialName = params.materialName || undefined;
    query.specs = params.specs || undefined;
    query.unit = params.unit || undefined;
    query.pageIndex = pageIndex;
    query.pageSize = pageSize;
    const result = await _materialServiceProxy.pagedMaterialsGet(query);
    return {
      items: result.items || [],
      totalCount: result.totalCount || 0,
    };
  } catch (error) {
    message.error('查询失败');
    return { items: [], totalCount: 0 };
  } finally {
    closeFullLoading();
  }
}

function buildCreateDto(request: Record<string, any>) {
  const materialCode = trimValue(request.materialCode);
  const dto = new MaterialCreateDto();
  dto.materialCode = materialCode;
  dto.materialName = trimValue(request.materialName);
  dto.specs = trimValue(request.specs);
  dto.unit = trimValue(request.unit);
  dto.typeCode = resolveTypeCode(materialCode);
  dto.typeName = '-';
  dto.safetyStock = 0;
  dto.fullBoxCount = 0;
  dto.expiryDate = 0;
  dto.isQCPJ = false;
  dto.isPPAP = false;
  return dto;
}

function buildUpdateDto(request: Record<string, any>, record: MaterialDto) {
  const materialCodeNew = trimValue(request.materialCodeNew);
  const dto = new MaterialUpdateDto();
  dto.materialCodeNew = materialCodeNew;
  dto.materialNameNew = trimValue(request.materialNameNew);
  dto.specsNew = trimValue(request.specsNew);
  dto.unitNew = trimValue(request.unitNew);
  dto.typeCodeNew = resolveTypeCode(materialCodeNew, record.typeCode);
  dto.typeNameNew = record.typeName?.trim() || '-';
  dto.isHBNew = record.isHB;
  dto.safetyStockNew = record.safetyStock ?? 0;
  dto.fullBoxCount = 0;
  dto.expiryDateNew = record.expiryDate ?? 0;
  dto.isQCPJNew = record.isQCPJ ?? false;
  dto.isPPAPNew = record.isPPAP ?? false;
  return dto;
}

export async function createMaterialAsync({
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  try {
    const res = await _materialServiceProxy.materialAdd(buildCreateDto(request));
    if (!res.success) {
      message.error(res.message || '新增失败');
      throw new Error(res.message);
    }
    message.success(t('common.operationSuccess'));
    resetFields();
    closeModal();
  } catch (error) {
    message.error(getErrorMessage(error, '新增失败'));
    throw error;
  } finally {
    changeOkLoading(false);
  }
}

export async function updateMaterialAsync({
  id,
  record,
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  try {
    const res = await _materialServiceProxy.materialUpdate(id, buildUpdateDto(request, record));
    if (!res.success) {
      message.error(res.message || '更新失败');
      throw new Error(res.message);
    }
    message.success(t('common.operationSuccess'));
    resetFields();
    closeModal();
  } catch (error) {
    message.error(getErrorMessage(error, '更新失败'));
    throw error;
  } finally {
    changeOkLoading(false);
  }
}

export async function deleteMaterialAsync({ materialCode, reload }) {
  openFullLoading();
  try {
    const res = await _materialServiceProxy.materialDel(materialCode?.trim());
    if (!res.success) {
      message.error(res.message || '删除失败');
      return;
    }
    message.success(t('common.operationSuccess'));
    reload();
  } catch (error) {
    message.error(getErrorMessage(error, '删除失败'));
  } finally {
    closeFullLoading();
  }
}
