<template>
    <div class="components-input-demo-presuffix">
        <Header numb="创建出库任务">
            <template #action>
                <a-button
                    size="small"
                    :type="showAllStocks ? 'primary' : 'default'"
                    @click="toggleShowAll"
                >
                    {{ showAllStocks ? '只看未下任务' : '所有库存信息' }}
                </a-button>
            </template>
        </Header>
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

        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1>出库库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="outCellCode" placeholder="出库库位" :allowClear="true"
                    @focus="focusFn" class="modern-input" ref="outCellInputRef">
                </a-input>
            </a-col>
        </a-row>

        <div >
            <p style="margin-left: 20px">库存信息:{{ dataSource.length }}
                <span v-if="hiddenCount > 0" style="color:#fa8c16; font-size:12px;"> (已隐藏 {{ hiddenCount }} 条已下出库任务库存)</span>
            </p>
            <a-table 
                ref="tableRef" 
                :dataSource="dataSource" 
                :columns="stockcolumns" 
                :pagination="false"
                :scroll="{ x: screenWidth }"
                rowKey="id"
                @row-click="onRowClick"
                :row-class-name="(record) => selectedRowId.value === record.id ? 'selected-row' : ''"
                :row-selection="rowSelection"
            >
            </a-table>
        </div>

        <div v-show="showtable">
            <div class="tab-bar">
                <a-button @click="createOutStockTask" type="primary" class="modern-btn">
                    创建出库任务
                </a-button>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
import { ref, h, onMounted, computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { stocksQuery, createStockTaskV2 } from './Stock';
// 移除API调用，保留前端界面
const barcodeGet = async (barcode) => {
  // 不调用接口，返回空数据
  return null;
};

const stocksCreateAndBindToCell = async (stockData) => {
  // 不调用接口，不执行任何操作
  return null;
};


import { PagedStockQueryDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue'
// 恢复表格列定义
const stockcolumns = [
  {
    title: '物料编码',
    dataIndex: 'materialCode',
    key: 'materialCode',
    align: 'center',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
    align: 'center',
  },
  {
    title: '规格',
    dataIndex: 'specs',
    key: 'specs',
    align: 'center',
  },
  {
    title: '箱号',
    dataIndex: 'processNo',
    key: 'processNo',
    align: 'center',
  },
  {
    title: '等级',
    dataIndex: 'grade',
    key: 'grade',
    align: 'center',
  },
  {
    title: '箱数',
    dataIndex: 'receivePkgOrBoxCount',
    key: 'receivePkgOrBoxCount',
    align: 'center',
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    align: 'center',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
    key: 'cellCode',
    align: 'center',
  },
  {
    title: '容器',
    dataIndex: 'boxCode',
    key: 'boxCode',
    align: 'center',
  },
];

const focus1 = ref<any>();
const outCellInputRef = ref<any>();
let goodsCode = ref<string>('');
let fliter = ref<string>('');
let outCellCode = ref<string>('');
var goods = ref([]
);
var allStocks = ref<any[]>([]);
let showAllStocks = ref(false);
var dataSource = computed(() => {
    if (showAllStocks.value) return allStocks.value;
    return allStocks.value.filter((s) => !s.hasTask);
});
const hiddenCount = computed(() => allStocks.value.length - dataSource.value.length);
let findtype = ref("barcode")
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let YHOne = ref();
let showtable = ref(true)
let goodheight = ref(36)
const tableRef = ref<any>();
let selectedRowId = ref<string>('');
let selectedStock = ref<any>(null);

// 响应式的行选择配置
const rowSelection = computed(() => ({
    selectedRowKeys: selectedRowId.value ? [selectedRowId.value] : [],
    onChange: (selectedRowKeys, selectedRows) => {
        if (selectedRows.length > 0) {
            selectedRowId.value = selectedRows[0].id;
            selectedStock.value = selectedRows[0];
            message.info(`已选择库存: ${selectedRows[0].materialName}`);
        } else {
            selectedRowId.value = '';
            selectedStock.value = null;
        }
    }
}));
onMounted(() => {
    console.log(tableRef.value?.$el?.querySelector('.ant-table-thead')?.clientHeight);
    if (tableRef.value?.$el?.querySelector('.ant-table-thead')) {
        YHOne.value = screenHeight.value - 42 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    }
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
        allStocks.value = res ? res.slice() : [];
        const visible = dataSource.value;
        const hidden = allStocks.value.length - visible.length;
        if (visible.length > 0) {
            // 自动选择第一条可见库存
            selectedRowId.value = visible[0].id;
            selectedStock.value = visible[0];
            message.info(`已自动选择库存: ${visible[0].materialName}`);
            if (hidden > 0) {
                message.warning(`已隐藏 ${hidden} 条已下出库任务的库存，点击右上角「所有库存信息」可查看`);
            }
        } else {
            // 重置选中状态
            selectedRowId.value = '';
            selectedStock.value = null;
            message.warning('符合条件的库存均已下出库任务');
        }
    }).catch((err) => {
        message.error(err.error.message);
    });

    setTimeout(() => {
        if (outCellInputRef.value) {
            outCellInputRef.value.focus();
        }
    }, 100);
}

// 处理表格行点击事件
const onRowClick = (record) => {
    selectedRowId.value = record.id;
    selectedStock.value = record;
    message.info(`已选择库存: ${record.materialName}`);
};

// 创建出库任务
const createOutStockTask = async () => {
    if (!selectedStock.value) {
        message.error("请选择一个库存");
        return;
    }
    if (!outCellCode.value) {
        message.error("请输入出库库位");
        return;
    }
    
    try {
        await createStockTaskV2(selectedStock.value.boxCode, selectedStock.value.cellCode, outCellCode.value).then((res) => {
            if (res.success == true) {
                // 创建出库任务
                message.success("创建任务成功");
                // 清空数据
                fliter.value = '';
                outCellCode.value = '';
                selectedRowId.value = '';
                selectedStock.value = null;
                allStocks.value = [];
            } else if(res.success == false){
                message.error(res.message);
            }else{
                message.error("接口推送异常，出库失败");
            }
        }).catch((error) => {
            message.error(error.error.message);
        });
    } catch (error) {
        message.error("出库任务创建失败");
    }
};

function toggleShowAll() {
    showAllStocks.value = !showAllStocks.value;
    selectedRowId.value = '';
    selectedStock.value = null;
}

//软件盘弹出屏蔽
function focusFn(e) {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
}

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

/* 现代化按钮样式 */
.modern-btn {
    margin: auto;
    height: 32px !important;
    border-radius: 6px !important;
    background: #1890ff !important;
    border: none !important;
    font-size: 14px !important;
    font-weight: 500 !important;
    color: white !important;
    box-shadow: none !important;
    transition: all 0.2s ease !important;
    
    &:hover {
        background: #40a9ff !important;
        box-shadow: none !important;
    }
    
    &:active {
        background: #096dd9 !important;
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

/* 选中行样式 */
.selected-row {
    background-color: #e6f7ff !important;
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