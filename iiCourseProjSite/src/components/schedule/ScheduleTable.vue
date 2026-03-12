<template>
  <div class="card bg-base-100 shadow-xl overflow-hidden">
    <!-- 工具栏 -->
    <div class="card-body py-4 border-b border-base-300 flex flex-wrap items-center justify-between gap-4">
      <div class="flex items-center gap-4">
        <select v-model="selectedWeek" class="select select-bordered select-sm">
          <option v-for="week in 20" :key="week" :value="week">第 {{ week }} 周</option>
        </select>
        <select v-model="selectedView" class="select select-bordered select-sm">
          <option value="week">周视图</option>
          <option value="day">日视图</option>
        </select>
        <span class="text-base-content/50 text-sm">{{ currentSemester }}</span>
      </div>
      <div class="flex items-center gap-4">
        <div class="flex items-center gap-2 text-sm">
          <button
            v-for="day in weekdays"
            :key="day.key"
            class="btn btn-xs"
            :class="selectedDay === day.key ? 'btn-primary' : 'btn-ghost'"
            @click="selectedDay = day.key"
          >
            {{ day.shortName }}
          </button>
        </div>
        <div class="text-sm text-base-content/50 border-l border-base-300 pl-4">
          {{ dateRange }}
        </div>
      </div>
    </div>

    <!-- 课程表 - 周视图 -->
    <div v-if="selectedView === 'week'" class="overflow-x-auto">
      <table class="table table-zebra w-full min-w-[800px]">
        <thead>
          <tr class="bg-gradient-to-r from-primary to-secondary text-white">
            <th class="py-4 px-2 w-20">节次</th>
            <th v-for="day in weekdays" :key="day.key" class="py-4 px-2">
              <div>{{ day.name }}</div>
              <div class="text-xs opacity-80">{{ day.date }}</div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="period in periods" :key="period.num" class="border-b border-base-300">
            <td class="py-4 px-2 text-center bg-base-200">
              <div class="font-semibold text-base-content">{{ period.num }}</div>
              <div class="text-xs text-base-content/50">{{ period.time }}</div>
            </td>
            <td
              v-for="day in weekdays"
              :key="day.key"
              class="py-2 px-1 min-h-[80px]"
            >
              <div
                v-if="getClass(day.key, period.num)"
                :class="['rounded-lg p-3 text-sm h-full cursor-pointer hover:shadow-lg transition-all hover:scale-105', getClass(day.key, period.num)?.color]"
                @click="showClassDetail(getClass(day.key, period.num)!)"
              >
                <div class="font-semibold text-base-content">{{ getClass(day.key, period.num)?.name }}</div>
                <div class="text-base-content/70 text-xs mt-1">{{ getClass(day.key, period.num)?.room }}</div>
                <div class="text-base-content/50 text-xs">{{ getClass(day.key, period.num)?.teacher }}</div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 课程表 - 日视图 -->
    <div v-else class="p-6">
      <div class="space-y-4">
        <div
          v-for="period in periods"
          :key="period.num"
          class="flex gap-4 p-4 rounded-xl border border-base-200"
          :class="getClass(selectedDay, period.num) ? 'bg-base-100' : 'bg-base-50 opacity-50'"
        >
          <div class="w-20 text-center">
            <div class="font-bold text-lg text-base-content">{{ period.num }}</div>
            <div class="text-sm text-base-content/50">{{ period.time }}</div>
          </div>
          <div class="flex-1">
            <div
              v-if="getClass(selectedDay, period.num)"
              :class="['rounded-lg p-4 cursor-pointer hover:shadow-lg transition-all', getClass(selectedDay, period.num)?.color]"
              @click="showClassDetail(getClass(selectedDay, period.num)!)"
            >
              <div class="font-bold text-lg text-base-content">{{ getClass(selectedDay, period.num)?.name }}</div>
              <div class="flex items-center gap-4 mt-2 text-sm text-base-content/70">
                <span class="flex items-center gap-1">
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                    <circle cx="12" cy="10" r="3"></circle>
                  </svg>
                  {{ getClass(selectedDay, period.num)?.room }}
                </span>
                <span class="flex items-center gap-1">
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                    <circle cx="12" cy="7" r="4"></circle>
                  </svg>
                  {{ getClass(selectedDay, period.num)?.teacher }}
                </span>
              </div>
            </div>
            <div v-else class="h-full flex items-center justify-center text-base-content/30">
              无课程
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 图例 -->
    <div class="card-body py-4 border-t border-base-300">
      <div class="flex flex-wrap items-center gap-4 text-sm">
        <span class="text-base-content/50">课程类型：</span>
        <div class="flex items-center gap-2">
          <span class="w-4 h-4 rounded bg-orange-100 border-l-2 border-orange-400"></span>
          <span>数学类</span>
        </div>
        <div class="flex items-center gap-2">
          <span class="w-4 h-4 rounded bg-blue-100 border-l-2 border-blue-400"></span>
          <span>语言类</span>
        </div>
        <div class="flex items-center gap-2">
          <span class="w-4 h-4 rounded bg-green-100 border-l-2 border-green-400"></span>
          <span>编程类</span>
        </div>
        <div class="flex items-center gap-2">
          <span class="w-4 h-4 rounded bg-purple-100 border-l-2 border-purple-400"></span>
          <span>专业核心</span>
        </div>
        <div class="flex items-center gap-2">
          <span class="w-4 h-4 rounded bg-pink-100 border-l-2 border-pink-400"></span>
          <span>实验实践</span>
        </div>
      </div>
    </div>

    <!-- 课程详情弹窗 -->
    <dialog ref="detailModal" class="modal">
      <div class="modal-box" v-if="selectedClass">
        <div class="flex items-start justify-between mb-4">
          <h3 class="font-bold text-xl">{{ selectedClass.name }}</h3>
          <span :class="['badge', selectedClass.color.replace('bg-', 'badge-').replace('100', '').replace('border-l-4', '')]">
            {{ selectedClass.type }}
          </span>
        </div>
        <div class="space-y-4">
          <div class="flex items-center gap-3 p-3 bg-base-200 rounded-lg">
            <div class="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                <circle cx="12" cy="10" r="3"></circle>
              </svg>
            </div>
            <div>
              <div class="text-sm text-base-content/50">教室</div>
              <div class="font-medium">{{ selectedClass.room }}</div>
            </div>
          </div>
          <div class="flex items-center gap-3 p-3 bg-base-200 rounded-lg">
            <div class="w-10 h-10 rounded-lg bg-secondary/10 flex items-center justify-center text-secondary">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
            </div>
            <div>
              <div class="text-sm text-base-content/50">教师</div>
              <div class="font-medium">{{ selectedClass.teacher }}</div>
            </div>
          </div>
          <div class="flex items-center gap-3 p-3 bg-base-200 rounded-lg">
            <div class="w-10 h-10 rounded-lg bg-accent/10 flex items-center justify-center text-accent">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                <line x1="16" y1="2" x2="16" y2="6"></line>
                <line x1="8" y1="2" x2="8" y2="6"></line>
                <line x1="3" y1="10" x2="21" y2="10"></line>
              </svg>
            </div>
            <div>
              <div class="text-sm text-base-content/50">时间</div>
              <div class="font-medium">{{ selectedClass.weekdayName }} 第{{ selectedClass.period }}节</div>
            </div>
          </div>
          <div class="flex items-center gap-3 p-3 bg-base-200 rounded-lg">
            <div class="w-10 h-10 rounded-lg bg-info/10 flex items-center justify-center text-info">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="16" x2="12" y2="12"></line>
                <line x1="12" y1="8" x2="12.01" y2="8"></line>
              </svg>
            </div>
            <div>
              <div class="text-sm text-base-content/50">周次</div>
              <div class="font-medium">第 {{ selectedClass.weeks }} 周</div>
            </div>
          </div>
        </div>
        <div class="modal-action">
          <button class="btn btn-primary" @click="closeDetail">确定</button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="closeDetail">关闭</button>
      </form>
    </dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

const selectedWeek = ref(3);
const selectedView = ref('week');
const selectedDay = ref(1);
const detailModal = ref<HTMLDialogElement | null>(null);
const selectedClass = ref<any>(null);

const currentSemester = '2024-2025学年 第2学期';

const weekdays = [
  { key: 1, name: '周一', shortName: '一', date: '3/10' },
  { key: 2, name: '周二', shortName: '二', date: '3/11' },
  { key: 3, name: '周三', shortName: '三', date: '3/12' },
  { key: 4, name: '周四', shortName: '四', date: '3/13' },
  { key: 5, name: '周五', shortName: '五', date: '3/14' },
];

const periods = [
  { num: 1, time: '08:00' },
  { num: 2, time: '09:00' },
  { num: 3, time: '10:00' },
  { num: 4, time: '11:00' },
  { num: 5, time: '14:00' },
  { num: 6, time: '15:00' },
  { num: 7, time: '16:00' },
  { num: 8, time: '17:00' },
];

// 示例课程数据 - 不同周次有不同的课程
const allClasses = [
  // 第3周课程
  { weekday: 1, period: 1, name: '高等数学', room: 'A101', teacher: '张教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 3 },
  { weekday: 1, period: 3, name: '大学英语', room: 'B205', teacher: '李老师', color: 'bg-blue-100 border-l-4 border-blue-400', type: '语言类', weeks: '1-16', weekNum: 3 },
  { weekday: 2, period: 2, name: '程序设计', room: 'C301', teacher: '王教授', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 3 },
  { weekday: 2, period: 5, name: '数据结构', room: 'C302', teacher: '赵老师', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 3 },
  { weekday: 3, period: 1, name: '线性代数', room: 'A102', teacher: '刘教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 3 },
  { weekday: 3, period: 4, name: '物理实验', room: 'D401', teacher: '陈老师', color: 'bg-pink-100 border-l-4 border-pink-400', type: '实验实践', weeks: '3-14', weekNum: 3 },
  { weekday: 4, period: 2, name: '操作系统', room: 'C303', teacher: '周教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 3 },
  { weekday: 4, period: 6, name: '计算机网络', room: 'C304', teacher: '吴老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 3 },
  { weekday: 5, period: 1, name: '数据库原理', room: 'C305', teacher: '郑教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 3 },
  { weekday: 5, period: 3, name: '软件工程', room: 'C306', teacher: '孙老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 3 },

  // 第4周课程（部分课程变化）
  { weekday: 1, period: 1, name: '高等数学', room: 'A101', teacher: '张教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 4 },
  { weekday: 1, period: 3, name: '大学英语', room: 'B205', teacher: '李老师', color: 'bg-blue-100 border-l-4 border-blue-400', type: '语言类', weeks: '1-16', weekNum: 4 },
  { weekday: 2, period: 2, name: '程序设计', room: 'C301', teacher: '王教授', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 4 },
  { weekday: 2, period: 5, name: '数据结构', room: 'C302', teacher: '赵老师', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 4 },
  { weekday: 3, period: 1, name: '线性代数', room: 'A102', teacher: '刘教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 4 },
  { weekday: 3, period: 4, name: '物理实验', room: 'D401', teacher: '陈老师', color: 'bg-pink-100 border-l-4 border-pink-400', type: '实验实践', weeks: '3-14', weekNum: 4 },
  { weekday: 4, period: 2, name: '操作系统', room: 'C303', teacher: '周教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 4 },
  { weekday: 4, period: 6, name: '计算机网络', room: 'C304', teacher: '吴老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 4 },
  { weekday: 5, period: 1, name: '数据库原理', room: 'C305', teacher: '郑教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 4 },
  { weekday: 5, period: 3, name: '软件工程', room: 'C306', teacher: '孙老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 4 },
  { weekday: 5, period: 5, name: '算法设计', room: 'C307', teacher: '钱教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '4-12', weekNum: 4 },

  // 第5周课程（更多变化）
  { weekday: 1, period: 1, name: '高等数学', room: 'A101', teacher: '张教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 5 },
  { weekday: 1, period: 3, name: '大学英语', room: 'B205', teacher: '李老师', color: 'bg-blue-100 border-l-4 border-blue-400', type: '语言类', weeks: '1-16', weekNum: 5 },
  { weekday: 2, period: 2, name: '程序设计', room: 'C301', teacher: '王教授', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 5 },
  { weekday: 2, period: 5, name: '数据结构', room: 'C302', teacher: '赵老师', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 5 },
  { weekday: 3, period: 1, name: '线性代数', room: 'A102', teacher: '刘教授', color: 'bg-orange-100 border-l-4 border-orange-400', type: '数学类', weeks: '1-16', weekNum: 5 },
  { weekday: 3, period: 4, name: '物理实验', room: 'D401', teacher: '陈老师', color: 'bg-pink-100 border-l-4 border-pink-400', type: '实验实践', weeks: '3-14', weekNum: 5 },
  { weekday: 4, period: 2, name: '操作系统', room: 'C303', teacher: '周教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 5 },
  { weekday: 4, period: 6, name: '计算机网络', room: 'C304', teacher: '吴老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 5 },
  { weekday: 5, period: 1, name: '数据库原理', room: 'C305', teacher: '郑教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '1-16', weekNum: 5 },
  { weekday: 5, period: 3, name: '软件工程', room: 'C306', teacher: '孙老师', color: 'bg-green-100 border-l-4 border-green-400', type: '编程类', weeks: '1-16', weekNum: 5 },
  { weekday: 5, period: 5, name: '算法设计', room: 'C307', teacher: '钱教授', color: 'bg-purple-100 border-l-4 border-purple-400', type: '专业核心', weeks: '4-12', weekNum: 5 },
  { weekday: 1, period: 5, name: '机器学习', room: 'C308', teacher: '周教授', color: 'bg-yellow-100 border-l-4 border-yellow-400', type: '专业选修', weeks: '5-16', weekNum: 5 },
];

const dateRange = computed(() => {
  const startDate = new Date(2025, 2, 10); // 3月10日
  startDate.setDate(startDate.getDate() + (selectedWeek.value - 1) * 7);
  const endDate = new Date(startDate);
  endDate.setDate(endDate.getDate() + 6);
  return `${startDate.getMonth() + 1}月${startDate.getDate()}日 - ${endDate.getMonth() + 1}月${endDate.getDate()}日`;
});

// 根据当前选择的周次获取课程
const currentWeekClasses = computed(() => {
  return allClasses.filter(c => c.weekNum === selectedWeek.value);
});

function getClass(weekday: number, period: number) {
  const cls = currentWeekClasses.value.find(c => c.weekday === weekday && c.period === period);
  if (cls) {
    return {
      ...cls,
      weekdayName: weekdays.find(d => d.key === cls.weekday)?.name || ''
    };
  }
  return null;
}

function showClassDetail(cls: any) {
  selectedClass.value = cls;
  detailModal.value?.showModal();
}

function closeDetail() {
  detailModal.value?.close();
  selectedClass.value = null;
}

// 监听周次变化，确保当前周次有数据
watch(selectedWeek, (newWeek) => {
  // 如果切换到没有数据的周次，可以在这里添加提示或默认行为
  const hasData = allClasses.some(c => c.weekNum === newWeek);
  if (!hasData) {
    // 可以在这里显示提示，但暂时不做处理
    console.log(`第${newWeek}周暂无课程数据`);
  }
});
</script>
