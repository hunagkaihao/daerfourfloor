<template>
    <div class="components-input-demo-presuffix">
        <Header numb="创建托盘搬运任务"></Header>
        <!--<a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">扫描托盘:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" placeholder="扫描托盘码" @keyup.enter="scanboxCode" :allowClear="true"
                    @focus="focusFn">
                    <template #suffix>
                        <scan-outlined />
                    </template>
                </a-input>
            </a-col>
        </a-row>-->
        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">当前库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="cellCode" placeholder="扫描当前库位" @keyup.enter="scancellCode" :allowClear="true"
                    @focus="focusFn" ref="cellInputRef" autofocus>
                    <template #suffix>
                        <scan-outlined />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        <LaneCellChips :list="laneCellStatusList" :current-cell-code="cellCode" />

        <div >
            <p style="margin-left: 20px">物料信息:{{ dataSource.length }}</p>
            <a-table 
                :dataSource="dataSource" 
                :columns="stockcolumns" 
                :pagination="false"
                :scroll="{ x: screenWidth }"
                rowKey="id"
                @row-click="onRowClick"
                :row-class-name="(record) => selectedRowId === record.id ? 'selected-row' : ''"
            >
            </a-table>
        </div>


        <div v-show="showtable">



            <div class="tab-bar">
                <a-button @click="createTask" type="primary" class="btn_4">
                    创建任务
                </a-button>

            </div>
        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
    <CellSelectModal @register="registerCellSelectModal"></CellSelectModal>
</template>
<script lang="ts" setup>
import { ref, h, onMounted,computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message, Tag } from 'ant-design-vue';
import { StockCreateDto, CellDto, CellLaneStatusDto } from '/@/services/ServiceProxies';
import { createStockTask, stocksGetInCell } from './Stock';
import { getLaneCellStatusByCellCode } from '/@/views/warehouse/cells/Cell';
import LaneCellChips from '../components/LaneCellChips.vue';
import Header from '../header/Header.vue'
import { useModal } from '/@/components/Modal';


const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
const [registerCellSelectModal, { openModal: openCellSelectModal }] = useModal();
let goodsCode = ref<string>('');
let cellCode = ref<string>('');
const laneCellStatusList = ref<CellLaneStatusDto[]>([]);

let boxCode = ref<string>('')
var goods = ref([]
);
var dataSource = ref([]
);
let showtable = ref(true)
let cell = new CellDto()
let selectedRowId = ref<string>('')
const cellInputRef = ref<any>();

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
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    align: 'center',
  },
  {
    title: '抽检状态',
    dataIndex: 'inspectionStatus',
    key: 'inspectionStatus',
    align: 'center',
    width: 80,
    customRender: ({ text }) => {
      const statusMap = {
        0: { label: '待检', color: 'default' },
        1: { label: '抽检中', color: 'processing' },
        2: { label: '合格', color: 'success' },
        3: { label: '不合格', color: 'error' },
        4: { label: '抽检完成', color: 'default' },
      };
      const status = statusMap[text];
      if (!status) return text ?? '-';
      return h(Tag, { color: status.color }, () => status.label);
    },
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

let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)

onMounted(() => {
    setTimeout(() => {
        if (cellInputRef.value) {
            cellInputRef.value.focus();
        }
    }, 100);
})
var lock = true
//扫描托盘码
async function scanboxCode() {
    
}

const fetchLaneCellStatus = async () => {
    if (!cellCode.value.trim()) {
        laneCellStatusList.value = [];
        return;
    }
    try {
        laneCellStatusList.value = await getLaneCellStatusByCellCode(cellCode.value);
    } catch (error: any) {
        laneCellStatusList.value = [];
        message.error(error?.error?.message || error?.message || '查询同巷道库位失败');
    }
};

//扫描库位
const scancellCode = async () => {
    await fetchLaneCellStatus();

    await stocksGetInCell(cellCode.value).then((res) => {
        dataSource.value.length = 0
        res.forEach((e) => {
            dataSource.value.push(e)
        })
        // 自动选择第一个容器
        if (res.length > 0) {
            selectedRowId.value = res[0].id
            boxCode.value = res[0].boxCode
            message.info(`已自动选择容器: ${res[0].boxCode}`)
        } else {
            // 重置选中状态
            selectedRowId.value = ''
            boxCode.value = ''
        }
    }).catch((err) => {
        message.error(err.error.message)
    })
}

// 处理表格行点击事件
const onRowClick = (record) => {
    selectedRowId.value = record.id
    boxCode.value = record.boxCode
    message.info(`已选择容器: ${record.boxCode}`)
}

function resetPage() {
    cellCode.value = ''
    boxCode.value = ''
    goodsCode.value = ''
    selectedRowId.value = ''
    goods.value = []
    dataSource.value = []
    laneCellStatusList.value = []
}

const createTask = async () => {
    if (cellCode.value == '') {
        message.error("没有当前库位信息")
        return
    }
    if (boxCode.value == '') {
        message.error("请先绑定物料容器")
        return
    }
    try{
        await createStockTask(boxCode.value, cellCode.value, '').then((res) => {
            if (res.success == true) {
                message.success(res.message)
                resetPage()
            } else if(res.success == false){
                message.error(res.message)
            }else{
                message.error("接口推送异常，入库失败")
            }
        }).catch((error) => {
            message.error(error.error.message)
        })
    }catch(err){
        message.error(err)
    }
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
.btn_4 {
    margin: auto;
    margin-top: 10px;
    margin-bottom: 10px;
}

.htext {
    text-align: center;
    line-height: 30px;
}

.tab-bar {
    display: flex;
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    height: 49px;
    background-color: #fff;
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

// 选中行样式
.selected-row {
    background-color: #e6f7ff !important;
}
</style>
