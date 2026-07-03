<template>
    <div class="components-input-demo-presuffix">
        <Header numb="库存查询"></Header>
        <a-row class="input-row">
            <a-col :span="9" :offset="1">
                <a-select v-model:value="findtype" class="modern-select" style="width: 90%;">
                    <a-select-option value="materialCode">物料编号</a-select-option>
                    <a-select-option value="cellCode">库位</a-select-option>
                    <a-select-option value="barcode">收料码</a-select-option>
                </a-select>
            </a-col>
            <a-col :span="13">
                <a-input v-model:value="fliter" placeholder="扫描" @keyup.enter="scancellCode" ref="focus1"
                    :allowClear="true" @focus="focusFn" class="modern-input">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>

        <div >
            <p style="margin-left: 20px">库存信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="stockcolumns" :pagination="true"
                :scroll="{ x: screenWidth }">
                <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'operation'">
                        <span @click="deleteStock(record, index)" style="color: #ff4d4f">删除</span>
                    </template>
                </template>
            </a-table>



        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
</template>
<script lang="ts" setup>
import { ref, h, onMounted } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { stocksQuery, stockRemoveDirect } from './Stock';
import type { Recordable } from '/@/types'
// 移除API调用，保留前端界面
const barcodeGet = async (barcode) => {
  // 不调用接口，返回空数据
  return null;
};

const stocksCreateAndBindToCell = async (stockData) => {
  // 不调用接口，不执行任何操作
  return null;
};


import { StockCreateDto,PagedStockQueryDto } from '/@/services/ServiceProxies';
import { useMessage } from '/@/hooks/web/useMessage';
import Header from '../header/Header.vue'
import { useModal } from '/@/components/Modal';
// 恢复表格列定义
const stockcolumns = [
  {
    title: '物料编码',
    dataIndex: 'materialCode',
    key: 'materialCode',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
  },
  {
    title: '规格',
    dataIndex: 'specs',
    key: 'specs',
  },
  {
    title: '数量',
    dataIndex: 'receiveTotalCount',
    key: 'receiveTotalCount',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
    key: 'cellCode',
  },
  {
    title: '操作',
    key: 'operation',
    width: 50,
    slots: {
      customRender: 'bodyCell'
    }
  }
];
// 已删除的StockDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };

const { createConfirm } = useMessage();
const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
const focus1 = ref<any>();
let goodsCode = ref<string>('');
let fliter = ref<string>('');
var goods = ref([]
);
var dataSource = ref<any[]>([]
);
let findtype = ref("barcode")
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let YHOne = ref();
let showtable = ref(true)
let goodheight = ref(36)
const tableRef = ref<any>();
onMounted(() => {
    console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    YHOne.value = screenHeight.value - 42 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    window.onresize = () => {
        var showHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
        console.log(showHeight - screenHeight.value)
        if (showHeight - screenHeight.value >= 0) {

            showtable.value = true
            goodheight.value = 36
        } else {
            showtable.value = false
            goodheight.value = 60
        }
    }
    
    // 页面加载完成后，自动聚焦到输入框
    setTimeout(() => {
        if (focus1.value) {
            focus1.value.focus();
        }
    }, 100);
})
var lock = true


const scancellCode = async () => {
    let queryValue = fliter.value;
    
    // 处理@分隔格式的条码
    if (queryValue.includes('@')) {
        const parts = queryValue.split('@');
        if (parts.length >= 5) {
            // 截取第5个字段（索引4）作为查询值
            queryValue = parts[4];
            message.info(`已解析条码，使用查询值：${queryValue}`);
        }
    }
    
    // 处理逗号分隔格式的条码
    if (queryValue.includes(',')) {
        const parts = queryValue.split(',');
        if (parts.length > 0) {
            // 获取第一个逗号前的数据作为查询值
            queryValue = parts[0];
            message.info(`已解析条码，使用查询值：${queryValue}`);
        }
    }
    
    var params =  new PagedStockQueryDto();
    if(findtype.value == "barcode" ){
        params.barcode = queryValue;
    }
    if(findtype.value == "cellCode" ){
        params.cellCode = fliter.value
    }
    if(findtype.value == "materialCode" ){
        params.materialCode = queryValue;
    }
    
    await stocksQuery(params).then((res) => {
        dataSource.value.length = 0;
        res.forEach((e) => {
            dataSource.value.push(e);
        });
    }).catch((err) => {
        message.error(err.error.message);
    });
}

const openGoodinfo = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};


const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};

//软件盘弹出屏蔽
function focusFn(e) {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
}

const deleteStock = (record: any, index: number) => {
    createConfirm({
        title: '确认删除',
        content: '确定要删除这条库存记录吗？',
        iconType: 'warning',
        onOk: async () => {
            try {
                // 调用删除API
                await stockRemoveDirect(record.id);
                // 从数据源中移除
                dataSource.value.splice(index, 1);
                message.success('删除成功');
            } catch (error) {
                message.error('删除失败');
            }
        },
    });
};

</script>

<style scoped lang="less">

/* 输入行样式 */
.input-row {
    margin: 10px 0;
    padding: 0 16px;
}

/* 文字标签样式 */
.htext {
    text-align: center;
    line-height: 32px;
    
    h1 {
        color: #333333;
        font-size: 14px;
        font-weight: 500;
        margin: 0;
        letter-spacing: 0.3px;
    }
}

/* 现代化输入框样式 */
.modern-input {
    height: 32px;
    border-radius: 6px !important;
    border: 1px solid #d9d9d9 !important;
    background: #ffffff !important;
    transition: all 0.2s ease !important;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;
    
    &:focus,
    &:hover {
        border-color: #1890ff !important;
        box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
    }
    
    &::placeholder {
        color: rgba(0, 0, 0, 0.45) !important;
        font-weight: 400;
    }
}

/* 现代化选择框样式 */
.modern-select {
    height: 32px;
    border-radius: 6px !important;
    border: 1px solid #d9d9d9 !important;
    background: #ffffff !important;
    transition: all 0.2s ease !important;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;
    
    &:focus,
    &:hover {
        border-color: #1890ff !important;
        box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
    }
}

/* 扫描图标样式 */
.scan-icon {
    color: #1890ff !important;
    font-size: 18px;
    transition: all 0.2s ease;
    
    &:hover {
        color: #40a9ff !important;
    }
}

/* 底部操作栏样式 */
.tab-bar {
    display: flex;
    align-items: center;
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    height: 60px;
    background: #ffffff;
    border-top: 1px solid #f0f0f0;
    box-shadow: none;
    padding: 0 16px;
}

::v-deep(.ant-table-thead > tr > th) {
    padding: 5px 0px;

}

::v-deep(.ant-table-tbody > tr > td) {
    padding: 5px 0px;
}

::v-deep(.ant-card-head) {
    padding: 0px;
    font-size: 14px;
    min-height: 0px;
}

::v-deep(.ant-card-head-title) {
    padding: 0px;
    white-space: normal;
}

::v-deep(.ant-card-extra) {
    padding: 0px;
}

::v-deep(.ant-card-body) {
    padding: 0 2px;
}

::v-deep(.ant-input-number-input) {
    height: 22px;
}

::v-deep(.ant-table-header.ant-table-hide-scrollbar) {
    margin-bottom: -20px;
    padding-bottom: 10px;
    overflow: scroll;
    opacity: 1;
}

::v-deep(.ant-table-hide-scrollbar) {
    scrollbar-color: initial !important;
}
::v-deep(.ant-table-placeholder){
    padding: 0 0px;
}
p {
    margin-bottom: 0em
}
</style>
