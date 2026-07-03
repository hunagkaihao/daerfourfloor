
import moment from 'moment';
import {
    PickListServiceProxy,
    StockServiceProxy,
    DepartmentServiceProxy,
    CellServiceProxy,
  } from '/@/services/ServiceProxies';
  const _PickNotifierServiceProxy = new PickListServiceProxy();
  const _StockServiceProxy = new StockServiceProxy();
  const _DepartmentServiceProxy = new DepartmentServiceProxy();
  const _CellServiceProxy = new CellServiceProxy();
  const _OutboundListServiceProxy = new CellServiceProxy();
  const _PickListServiceProxy = new PickListServiceProxy();
 export function pagedList(pickListCode: string | undefined, pageIndex: number, pageSize: number, skipCount: number | undefined, maxResultCount: number | undefined 
    ): Promise<any> {
      const queryDto = {
        pickListCode: pickListCode,
        pageIndex: pageIndex,
        pageSize: pageSize,
        skipCount: skipCount,
        maxResultCount: maxResultCount
      };
      return _PickListServiceProxy.pagedlist(queryDto);
    }
    //查询部门
    export function allDepartmentsGet(
    ): Promise<any> {
      return _DepartmentServiceProxy.allDepartmentsGet();
    }
    //释放库存
    export function releasePickStock(
      pickItemId
    ): Promise<any> {
      return _PickNotifierServiceProxy.releasePickStock(pickItemId);
    }
  //查询通知单
  export function pickItemsGet(
    param
  ): Promise<any> {
    return _PickNotifierServiceProxy.pickItemsGet(param);
  }
   //查询通知单
   export function getCellByCheck(
    barcode,boxCode,checkType
  ): Promise<any> {
    return _CellServiceProxy.getCellByCheck(barcode,boxCode,checkType);
  }
    //分页查询通知单
    export function pagedPickItemsGet(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.pickItemsForOutboundPaged(param);
    }
//分页查询库存明细
    export function stockDetailByCheckNoAreaPaged(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.stockDetailByCheckNoAreaPaged(param);
    }
    //分页查询全部库存明细
    export function autoAllocateStockDetailWithTotal(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.autoAllocateStockDetailWithTotal(param);
    }
    //批量自动分配
    export function batchAutoAllocateStockDetail(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.batchAutoAllocateStockDetail(param);
    }
    //批量人工分配
    export function batchAutoAllocateStockDetailWithTotal(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.batchAutoAllocateStockDetailWithTotal(param);
    }
    //添加出库单
    export function add(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.add(param);
    }
    //自动分配
  export function autoAllocateStockDetail(
      param
    ): Promise<any> {
      return _OutboundListServiceProxy.autoAllocateStockDetail(param);
    }
  //获取库位
  export function pickSuggestionGet(
    param
  ): Promise<any> {
    return _PickNotifierServiceProxy.pickStocksGet(param);
  }
    //锁定库位
    export function unbindCell(
      param
    ): Promise<any> {
      return _CellServiceProxy.unbindCell(param);
    }
    //获取所有库位
    export function cellsWithMaterial(
      param,uniqueCode
    ): Promise<any> {
      return _StockServiceProxy.cellsWithMaterial(param,uniqueCode);
    }
  //分配分拨墙库位
  export function getCellByWall(

  ): Promise<any> {
    return _CellServiceProxy.getCellByWall();
  }
  //出库
  export function pickOut(
    pickListCode,pickItemUniqueCode,param
  ): Promise<any> {
    return _PickNotifierServiceProxy.pickOut(pickListCode,pickItemUniqueCode,param);
  }
  //241022创建下架任务
  export function pickOutDown(
    startCellCode,endCellCode,pickListCode,pickItemUniqueCode,operatorName
  ): Promise<any> {
    return _PickNotifierServiceProxy.pickOutDown(startCellCode,endCellCode,pickListCode,pickItemUniqueCode,operatorName);
  }
//241105获取库位
export function getCellByPickOut(
  barCode,boxCode,pickListCode,pickItemUniqueCode
): Promise<any> {
  return _CellServiceProxy.getCellByPickOut(barCode,boxCode,pickListCode,pickItemUniqueCode);
}
  export const celldataSource = [
    {
      name: '01-01-01',
      age: '外壳EFM-210',
      address: '20240515',
      tags: '5000',
    },
   {
      name: '01-01-02',
      age: '外壳EFM-210',
      address: '20240515',
      tags: '5000',
    },
    {
    name: '01-01-03',
    age: '外壳EFM-210',
    address: '20240515',
    tags: '5000',
    },
    {
        name: '01-01-04',
        age: '外壳EFM-210',
        address: '20240515',
        tags: '5000',
    }
  ];
  export const hiscolumns = [
    {
      title: '节点步骤信息',
      dataIndex: 'stepNode',
      key: 'stepNode"',
      align: "center",
      width: 140,
    },
    {
      title: '料箱编号',
      dataIndex: 'boxCode',
      key: 'boxCode',
      align: "center",
      width: 70,
    },
    {
      title: '数量',
      dataIndex: 'outboundQuantity',
      key: 'outboundQuantity',
      align: "center",
      width: 70,
    },
     {
      title: '操作者',
      dataIndex: 'operator',
      key: 'operator',
      align: "center",
      width: 70,
    },
 {
      title: '操作时间',
      dataIndex: 'operationTime',
      key: 'operationTime',
      customRender: ({ text }) => {
            return moment(text).format('YYYY-MM-DD HH:mm:ss');
          },
      align: "center",
      width: 70,
    },

  ];
  export const columns = [
    {
      title: '领料单号',
      dataIndex: 'pickListCode',
      key: 'pickListCode',
      align: "center",
      width: 75,
    },
    {
      title: '物料编码',
      dataIndex: 'materialCode',
      key: 'materialCode',
      align: "center",
      width: 75,
    },
    {
      title: '物料',
      dataIndex: 'materialName',
      key: 'materialName',
      align: "center",
      width: 120,
    },
{
      title: '物料规格',
      dataIndex: 'specs',
      key: 'specs',
      align: "center",
      width: 120,
    },
    {
      title: '单位',
      dataIndex: 'unit',
      key: 'unit',
      align: "center",
      width: 70,
    },
    {
      title: '生产批号||成品名称',
      dataIndex: 'batchNo',
      key: 'batchNo',
      align: "center",
      width: 100,
    },
    {
      title: '成品编号',
      dataIndex: 'goodsCode',
      key: 'goodsCode',
      align: "center",
      width: 70,
    },
     {
      title: '成品规格',
      dataIndex: 'goodsSpecs',
      key: 'goodsSpecs',
      align: "center",
      width: 70,
    },
 {
      title: '领用类型',
      dataIndex: 'pickType',
      key: 'pickType',
      align: "center",
      width: 70,
    },
    {
      title: '应领数',
      key: 'countToPick',
      dataIndex: 'countToPick',
      ellipsis: true,
      align: "center",
      width: 60,
    },
    {
      title: '已领数量',
      key: 'pickedCount',
      dataIndex: 'pickedCount',
      ellipsis: true,
      align: "center",
      width: 70,
    },
    {
      title: '未领数',
      key: 'unpickedCount',
      dataIndex: 'unpickedCount',
      ellipsis: true,
      align: "center",
      width: 60,
    },
    {
      title: '车间开单人',
      key: 'workshopOrderCreator',
      dataIndex: 'workshopOrderCreator',
      ellipsis: true,
      align: "center",
      width: 70,
    },
     {
      title: '时间',
      dataIndex: 'creationTime',
      key: 'creationTime',
      customRender: ({ text }) => {
            return moment(text).format('YYYY-MM-DD');
          },
      align: "center",
      width: 70,
    },
    // {
    //   title: '库位',
    //   dataIndex: 'cellCode',
    //   key: 'cellCode',
    //   align: "center",
    //   width: 120,
    // },
    // {
    //     title: '已领数',
    //     key: 'tags',
    //     dataIndex: 'tagss',
    //     ellipsis: true,
    //     align: "center",
    //     width: 80,
    //   },
    // {
    //   title: '操作',
    //   key: 'operation',
    //   fixed: 'right',
    //   align: "center",
    //   width: 60,
    //   slots: {
    //           customRender: 'bodyCell'
    //       }
    // },
  ];
  export const ordercolumns = [
    {
      title: '物料编号',
      dataIndex: 'materialCode',
      key: 'materialCode',
      align: "center",
      width: 75,
    },
    {
      title: '物料名称',
      dataIndex: 'materialName',
      key: 'materialName',
      align: "center",
      width: 120,
    },
{
      title: '规格型号',
      dataIndex: 'specs',
      key: 'specs',
      align: "center",
      width: 120,
    },
    {
      title: '生产批号',
      dataIndex: 'batchNo',
      key: 'batchNo',
      align: "center",
      width: 70,
    },
     {
      title: '领料单号',
      key: 'pickListCode',
      dataIndex: 'pickListCode',
      align: "center",
      width: 60,
    },
       {
      title: '领用类型',
      key: 'pickType',
      dataIndex: 'pickType',
      align: "center",
      width: 60,
    },
    {
      title: '领用单位',
      key: 'department',
      dataIndex: 'department',
      align: "center",
      width: 60,
    },
   {
      title: '领用数量',
      dataIndex: 'countToPick',
      key: 'countToPick',
      align: "center",
      width: 70,
    },
    {
      title: '未领数量',
      dataIndex: 'unpickedCount',
      key: 'unpickedCount',
      align: "center",
      width: 70,
    },
    // {
    //   title: '操作',
    //   key: 'operation',
    //   fixed: 'right',
    //   align: "center",
    //   width: 60,
    //   slots: {
    //           customRender: 'bodyCell'
    //       }
    // },
  ];
    export const detailcolumns = [
    {
      title: '存货编号',
      dataIndex: 'materialCode',
      key: 'materialCode',
      align: "center",
      width: 75,
    },
    {
      title: '存货名称',
      dataIndex: 'materialName',
      key: 'materialName',
      align: "center",
      width: 120,
    },
{
      title: '规格型号',
      dataIndex: 'specs',
      key: 'specs',
      align: "center",
      width: 120,
    },
    {
      title: '生产批号',
      dataIndex: 'batchNo',
      key: 'batchNo',
      align: "center",
      width: 120,
    },
    {
      title: '单位',
      dataIndex: 'unit',
      key: 'unit',
      align: "center",
      width: 70,
    },
    {
      title: '检验编号',
      key: 'checkNo',
      dataIndex: 'checkNo',
      align: "center",
      width: 60,
    },
   {
      title: '出库数量',
      dataIndex: 'quantity',
      key: 'quantity',
      align: "center",
      width: 70,
    },
    {
      title: '操作',
      key: 'operation',
      fixed: 'right',
      align: "center",
      width: 60,
      slots: {
              customRender: 'bodyCell'
          }
    },
  ];
   export const columns2 = [
    {
      title: '检验批号',
      dataIndex: 'checkNo',
      key: 'checkNo',
      align: "center",
      width: 70,
    },
    {
      title: '结存数量',
      dataIndex: 'totalCount',
      key: 'totalCount',
      align: "center",
      width: 75,
    },
    {
      title: '未分配结存数量',
      dataIndex: 'unallocatedCount',
      key: 'unallocatedCount',
      align: "center",
      width: 120,
    },

    {
      title: '库位',
      key: 'cellArea',
      dataIndex: 'cellArea',
      ellipsis: true,
      align: "center",
      width: 60,
    },

  ];
   export const columns3 = [
    {
      title: '检验批号',
      dataIndex: 'checkNo',
      key: 'checkNo',
      align: "center",
      width: 70,
    },
    {
      title: '结存数量',
      dataIndex: 'unallocatedCount',
      key: 'unallocatedCount',
      align: "center",
      width: 75,
    },
    {
      title: '分配数量',
      dataIndex: 'allocatedCount',
      key: 'allocatedCount',
      align: "center",
      //defaultValue: 0,
      width: 120,
      slots: {
       customRender: 'bodyCell'
          }
    },
    {
      title: '库位',
      key: 'cellArea',
      dataIndex: 'cellArea',
      ellipsis: true,
      align: "center",
      width: 60,
    },

  ];
   export const columns4 = [
    {
      title: '物料名称',
      dataIndex: 'materialName',
      key: 'materialName',
      align: "center",
      width: 70,
    },
    {
      title: '物料编号',
      dataIndex: 'materialCode',
      key: 'materialCode',
      align: "center",
      width: 70,
    },
    {
      title: '检验批号',
      dataIndex: 'checkNo',
      key: 'checkNo',
      align: "center",
      width: 70,
    },
    {
      title: '结存数量',
      dataIndex: 'unallocatedCount',
      key: 'unallocatedCount',
      align: "center",
      width: 75,
    },
    {
      title: '分配数量',
      dataIndex: 'allocatedCount',
      key: 'allocatedCount',
      align: "center",
      //defaultValue: 0,
      width: 120,
      slots: {
       customRender: 'bodyCell'
          }
    },
    {
      title: '库位',
      key: 'cellArea',
      dataIndex: 'cellArea',
      ellipsis: true,
      align: "center",
      width: 60,
    },

  ];
     export const autocolumns = [
    

    {
      title: '检验批号',
      dataIndex: 'checkNo',
      key: 'checkNo',
      align: "center",
      width: 70,
    },
  
    {
      title: '未分配结存数量',
      dataIndex: 'unallocatedCount',
      key: 'unallocatedCount',
      align: "center",
      width: 120,
    },
     {
      title: '分配数量',
      dataIndex: 'allocatedCount',
      key: 'allocatedCount',
      align: "center",
      
      width: 75,
    },
    {
      title: '库位',
      key: 'cellArea',
      dataIndex: 'cellArea',
      ellipsis: true,
      align: "center",
      width: 60,
    },

  ];
  export const outcolumns = [
    {
      title: '库位',
      dataIndex: 'cellCode',
      key: 'name',

      align: "center",
      
    },
    {
      title: '检验编号',
      dataIndex: 'checkNo',
      key: 'age',

      align: "center",
    },
    {
      title: '检验日期',
      dataIndex: 'stockInDate',
      key: 'stockInDate',
      align: "center",
    },
    {
      title: '当前库存',
      key: 'tags',
      dataIndex: 'stockCount',
      align: "center",
      width: 80,
    },
    {
        title: '本次领料',
        key: 'pickCount',
        dataIndex: 'pickCount',

        align: "center",
        width: 80,
      },
  ];
  export const cellcolumns = [
    {
      title: '库位编号',
      dataIndex: 'cellCode',
      key: 'cellCode',
      align: "center",
    },
    {
      title: '检验日期',
      dataIndex: 'stockInDate',
      key: 'stockInDate',
      align: "center",
    },
    {
      title: '检验编号',
      dataIndex: 'checkNo',
      key: 'checkNo',
      align: "center",
    },
    {
      title: '数量',
      dataIndex: 'stockCount',
      key: 'stockCount',
      align: "center",
    },
  ];
  
  
  