<template>
    <div v-if="list.length > 0" class="lane-cell-panel">
        <div class="lane-cell-section">
            <span class="lane-cell-label">货态</span>
            <div class="lane-cell-chips">
                <span
                    v-for="item in sortedList"
                    :key="`${item.cellCode}-status`"
                    class="lane-chip"
                    :class="statusChipClasses(item)"
                    :title="statusChipTitle(item)"
                >
                    {{ item.lanePosition }}{{ getStatusShort(item.status) }}
                </span>
            </div>
        </div>
        <div class="lane-cell-section">
            <span class="lane-cell-label">运行</span>
            <div class="lane-cell-chips">
                <span
                    v-for="item in sortedList"
                    :key="`${item.cellCode}-run`"
                    class="lane-chip"
                    :class="runStatusChipClasses(item)"
                    :title="runStatusChipTitle(item)"
                >
                    {{ item.lanePosition }}{{ getRunStatusShort(item.runStatus) }}
                </span>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import type { CellLaneStatusDto } from '/@/services/ServiceProxies';

const props = withDefaults(
    defineProps<{
        list: CellLaneStatusDto[];
        currentCellCode?: string;
    }>(),
    {
        currentCellCode: '',
    }
);

const sortedList = computed(() =>
    [...props.list].sort((a, b) => (b.lanePosition ?? 0) - (a.lanePosition ?? 0))
);

const cellStatusLabelMap: Record<string, string> = {
    Full: '满货',
    Have: '有货',
    Nohave: '无货',
    Pallet: '空托盘',
};

const runStatusLabelMap: Record<string, string> = {
    '1': '禁用',
    '2': '空闲',
    '3': '运行',
    '4': '搬运任务',
    Disable: '禁用',
    Enable: '空闲',
    Run: '运行',
    Selected: '搬运任务',
};

const normalizeRunStatus = (runStatus?: string) => {
    const value = String(runStatus ?? '');
    if (value === '2' || value === 'Enable') return 'enable';
    if (value === '4' || value === 'Selected') return 'selected';
    if (value === '1' || value === 'Disable') return 'disable';
    if (value === '3' || value === 'Run') return 'run';
    return 'unknown';
};

const getStatusLabel = (status?: string) => cellStatusLabelMap[status || ''] || status || '-';
const getRunStatusLabel = (runStatus?: string) =>
    runStatusLabelMap[String(runStatus ?? '')] || runStatus || '-';

const getStatusShort = (status?: string) => {
    switch (status) {
        case 'Nohave': return '空';
        case 'Have': return '货';
        case 'Full': return '满';
        case 'Pallet': return '托';
        default: return '-';
    }
};

const getRunStatusShort = (runStatus?: string) => {
    switch (normalizeRunStatus(runStatus)) {
        case 'enable': return '闲';
        case 'selected': return '搬';
        case 'disable': return '禁';
        case 'run': return '运';
        default: return '-';
    }
};

const statusChipTitle = (item: CellLaneStatusDto) =>
    `${item.cellCode} 位${item.lanePosition} ${getStatusLabel(item.status)}`;

const runStatusChipTitle = (item: CellLaneStatusDto) =>
    `${item.cellCode} 位${item.lanePosition} ${getRunStatusLabel(item.runStatus)}`;

const statusChipClasses = (item: CellLaneStatusDto) => {
    const classes: string[] = [];
    if (item.cellCode === props.currentCellCode) {
        classes.push('lane-chip-current');
    }
    switch (item.status) {
        case 'Nohave': classes.push('lane-chip-nohave'); break;
        case 'Have': classes.push('lane-chip-have'); break;
        case 'Full': classes.push('lane-chip-full'); break;
        default: classes.push('lane-chip-default'); break;
    }
    return classes;
};

const runStatusChipClasses = (item: CellLaneStatusDto) => {
    const classes: string[] = [];
    if (item.cellCode === props.currentCellCode) {
        classes.push('lane-chip-current');
    }
    switch (normalizeRunStatus(item.runStatus)) {
        case 'enable': classes.push('lane-chip-idle'); break;
        case 'selected': classes.push('lane-chip-task'); break;
        case 'disable': classes.push('lane-chip-disable'); break;
        case 'run': classes.push('lane-chip-run'); break;
        default: classes.push('lane-chip-default'); break;
    }
    return classes;
};
</script>

<style scoped lang="less">
.lane-cell-panel {
    margin: 0 16px 4px;
}

.lane-cell-section {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 0;

    & + .lane-cell-section {
        margin-top: 3px;
    }
}

.lane-cell-label {
    flex-shrink: 0;
    width: 28px;
    font-size: 11px;
    color: #999;
    line-height: 20px;
}

.lane-cell-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 3px;
    flex: 1;
    min-width: 0;
}

.lane-chip {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 26px;
    height: 20px;
    padding: 0 3px;
    font-size: 10px;
    font-weight: 500;
    line-height: 1;
    border-radius: 3px;
    border: 1px solid transparent;
    white-space: nowrap;
}

.lane-chip-nohave,
.lane-chip-idle {
    background: #f6ffed;
    color: #389e0d;
    border-color: #b7eb8f;
}

.lane-chip-have {
    background: #fff7e6;
    color: #d46b08;
    border-color: #ffd591;
}

.lane-chip-full,
.lane-chip-task {
    background: #fff1f0;
    color: #cf1322;
    border-color: #ffa39e;
}

.lane-chip-default {
    background: #fafafa;
    color: #666;
    border-color: #d9d9d9;
}

.lane-chip-current {
    border-color: #1890ff !important;
    box-shadow: 0 0 0 1px #1890ff;
    font-weight: 700;
}

.lane-chip-run {
    font-style: italic;
}

.lane-chip-disable {
    opacity: 0.45;
    text-decoration: line-through;
}
</style>
