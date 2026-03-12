<template>
  <div class="card bg-base-100 shadow-xl overflow-hidden">
    <!-- 统计卡片 -->
    <div class="card-body border-b border-base-300">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="card bg-gradient-to-br from-primary to-secondary text-white">
          <div class="card-body p-4">
            <div class="text-sm opacity-80">本学期绩点</div>
            <div class="text-3xl font-bold">{{ currentGPA }}</div>
          </div>
        </div>
        <div class="card bg-blue-50 border border-blue-200">
          <div class="card-body p-4">
            <div class="text-sm text-blue-600">平均分</div>
            <div class="text-3xl font-bold text-blue-700">{{ currentAverage }}</div>
          </div>
        </div>
        <div class="card bg-green-50 border border-green-200">
          <div class="card-body p-4">
            <div class="text-sm text-green-600">已修学分</div>
            <div class="text-3xl font-bold text-green-700">{{ totalCredits }}</div>
          </div>
        </div>
        <div class="card bg-purple-50 border border-purple-200">
          <div class="card-body p-4">
            <div class="text-sm text-purple-600">课程数</div>
            <div class="text-3xl font-bold text-purple-700">{{ filteredScores.length }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 筛选器 -->
    <div class="card-body py-4 border-b border-base-300 flex flex-wrap items-center gap-4">
      <select v-model="selectedYear" class="select select-bordered select-sm">
        <option value="all">全部学年</option>
        <option value="2024-2025">2024-2025</option>
        <option value="2023-2024">2023-2024</option>
      </select>
      <select v-model="selectedSemester" class="select select-bordered select-sm">
        <option value="all">全部学期</option>
        <option value="1">第一学期</option>
        <option value="2">第二学期</option>
      </select>
      <select v-model="sortBy" class="select select-bordered select-sm">
        <option value="default">默认排序</option>
        <option value="score-desc">成绩从高到低</option>
        <option value="score-asc">成绩从低到高</option>
        <option value="credit-desc">学分从高到低</option>
      </select>
      <div class="flex-1"></div>
      <div class="flex items-center gap-2 text-sm text-base-content/50">
        <span class="w-3 h-3 rounded-full bg-success"></span> 优秀
        <span class="w-3 h-3 rounded-full bg-info"></span> 良好
        <span class="w-3 h-3 rounded-full bg-warning"></span> 中等
        <span class="w-3 h-3 rounded-full bg-error"></span> 及格
      </div>
    </div>

    <!-- 成绩表格 -->
    <div class="overflow-x-auto">
      <table class="table table-zebra w-full">
        <thead>
          <tr class="bg-base-200">
            <th class="py-3 px-4 text-left text-sm font-semibold text-base-content cursor-pointer hover:bg-base-300 transition-colors" @click="toggleSort('name')">
              课程名称
              <span v-if="sortBy === 'name'" class="ml-1">↓</span>
            </th>
            <th class="py-3 px-4 text-center text-sm font-semibold text-base-content cursor-pointer hover:bg-base-300 transition-colors" @click="toggleSort('credit')">
              学分
              <span v-if="sortBy === 'credit-desc'" class="ml-1">↓</span>
            </th>
            <th class="py-3 px-4 text-center text-sm font-semibold text-base-content cursor-pointer hover:bg-base-300 transition-colors" @click="toggleSort('score')">
              成绩
              <span v-if="sortBy === 'score-desc' || sortBy === 'score-asc'" class="ml-1">
                {{ sortBy === 'score-desc' ? '↓' : '↑' }}
              </span>
            </th>
            <th class="py-3 px-4 text-center text-sm font-semibold text-base-content">绩点</th>
            <th class="py-3 px-4 text-center text-sm font-semibold text-base-content">性质</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-base-300">
          <tr
            v-for="score in sortedScores"
            :key="score.name + score.semester"
            class="hover:bg-base-200 transition-colors cursor-pointer"
            @click="showDetail(score)"
          >
            <td class="py-4 px-4">
              <div class="font-medium text-base-content">{{ score.name }}</div>
              <div class="text-sm text-base-content/50">{{ score.semester }}</div>
            </td>
            <td class="py-4 px-4 text-center text-base-content">{{ score.credit }}</td>
            <td class="py-4 px-4 text-center">
              <span :class="['badge', getScoreClass(score.score)]">
                {{ score.score }}
              </span>
            </td>
            <td class="py-4 px-4 text-center text-base-content">{{ score.gpa }}</td>
            <td class="py-4 px-4 text-center">
              <span :class="['badge badge-sm', score.nature === '必修' ? 'badge-primary' : 'badge-ghost']">
                {{ score.nature }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 详情弹窗 -->
    <dialog ref="detailModal" class="modal">
      <div class="modal-box" v-if="selectedScore">
        <h3 class="font-bold text-lg mb-4">{{ selectedScore.name }}</h3>
        <div class="space-y-3">
          <div class="flex justify-between">
            <span class="text-base-content/60">学期</span>
            <span class="font-medium">{{ selectedScore.semester }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-base-content/60">学分</span>
            <span class="font-medium">{{ selectedScore.credit }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-base-content/60">成绩</span>
            <span :class="['badge', getScoreClass(selectedScore.score)]">{{ selectedScore.score }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-base-content/60">绩点</span>
            <span class="font-medium">{{ selectedScore.gpa }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-base-content/60">课程性质</span>
            <span class="badge badge-sm" :class="selectedScore.nature === '必修' ? 'badge-primary' : 'badge-ghost'">
              {{ selectedScore.nature }}
            </span>
          </div>
          <div class="flex justify-between">
            <span class="text-base-content/60">任课教师</span>
            <span class="font-medium">{{ selectedScore.teacher }}</span>
          </div>
        </div>
        <div class="modal-action">
          <button class="btn" @click="closeDetail">关闭</button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="closeDetail">关闭</button>
      </form>
    </dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const selectedYear = ref('all');
const selectedSemester = ref('all');
const sortBy = ref('default');
const detailModal = ref<HTMLDialogElement | null>(null);
const selectedScore = ref<any>(null);

const scores = [
  { name: '设计素描', credit: 3.0, score: 92, gpa: '4.0', nature: '必修', semester: '2024-2025-1', teacher: '张教授' },
  { name: '色彩构成', credit: 3.0, score: 88, gpa: '3.7', nature: '必修', semester: '2024-2025-1', teacher: '李老师' },
  { name: '平面构成', credit: 3.5, score: 95, gpa: '4.0', nature: '必修', semester: '2024-2025-1', teacher: '王教授' },
  { name: '艺术概论', credit: 2.0, score: 85, gpa: '3.3', nature: '必修', semester: '2024-2025-1', teacher: '赵老师' },
  { name: '数字图像处理', credit: 3.5, score: 90, gpa: '4.0', nature: '必修', semester: '2024-2025-2', teacher: '陈老师' },
  { name: '版式设计', credit: 3.0, score: 87, gpa: '3.7', nature: '必修', semester: '2024-2025-2', teacher: '刘老师' },
  { name: '插画设计', credit: 3.0, score: 94, gpa: '4.0', nature: '必修', semester: '2024-2025-2', teacher: '周教授' },
  { name: '品牌设计基础', credit: 3.5, score: 91, gpa: '4.0', nature: '必修', semester: '2024-2025-2', teacher: '吴老师' },
  { name: '摄影基础', credit: 2.0, score: 88, gpa: '3.7', nature: '必修', semester: '2024-2025-2', teacher: '郑教授' },
  { name: 'UI设计入门', credit: 3.0, score: 93, gpa: '4.0', nature: '选修', semester: '2024-2025-2', teacher: '孙老师' },
  { name: '设计史', credit: 2.0, score: 89, gpa: '3.7', nature: '选修', semester: '2023-2024-1', teacher: '钱教授' },
  { name: '创意思维', credit: 2.0, score: 91, gpa: '4.0', nature: '选修', semester: '2023-2024-1', teacher: '周老师' },
  { name: '手绘表现', credit: 3.0, score: 86, gpa: '3.3', nature: '必修', semester: '2023-2024-2', teacher: '吴教授' },
  { name: '计算机辅助设计', credit: 3.5, score: 90, gpa: '4.0', nature: '必修', semester: '2023-2024-2', teacher: '郑老师' },
];

const filteredScores = computed(() => {
  return scores.filter(score => {
    const yearMatch = selectedYear.value === 'all' || score.semester.startsWith(selectedYear.value);
    const semesterMatch = selectedSemester.value === 'all' || score.semester.endsWith(selectedSemester.value);
    return yearMatch && semesterMatch;
  });
});

const sortedScores = computed(() => {
  let result = [...filteredScores.value];
  switch (sortBy.value) {
    case 'score-desc':
      result.sort((a, b) => b.score - a.score);
      break;
    case 'score-asc':
      result.sort((a, b) => a.score - b.score);
      break;
    case 'credit-desc':
      result.sort((a, b) => b.credit - a.credit);
      break;
    case 'name':
      result.sort((a, b) => a.name.localeCompare(b.name, 'zh-CN'));
      break;
  }
  return result;
});

const currentGPA = computed(() => {
  const list = filteredScores.value;
  if (list.length === 0) return '0.00';
  const totalGPA = list.reduce((sum, s) => sum + parseFloat(s.gpa) * s.credit, 0);
  const totalCredits = list.reduce((sum, s) => sum + s.credit, 0);
  return (totalGPA / totalCredits).toFixed(2);
});

const currentAverage = computed(() => {
  const list = filteredScores.value;
  if (list.length === 0) return '0.0';
  const total = list.reduce((sum, s) => sum + s.score * s.credit, 0);
  const totalCredits = list.reduce((sum, s) => sum + s.credit, 0);
  return (total / totalCredits).toFixed(1);
});

const totalCredits = computed(() => {
  return filteredScores.value.reduce((sum, s) => sum + s.credit, 0).toFixed(1);
});

function getScoreClass(score: number) {
  if (score >= 90) return 'badge-success';
  if (score >= 80) return 'badge-info';
  if (score >= 70) return 'badge-warning';
  return 'badge-error';
}

function toggleSort(field: string) {
  if (field === 'score') {
    if (sortBy.value === 'score-desc') {
      sortBy.value = 'score-asc';
    } else {
      sortBy.value = 'score-desc';
    }
  } else if (field === 'credit') {
    sortBy.value = 'credit-desc';
  } else if (field === 'name') {
    sortBy.value = 'name';
  }
}

function showDetail(score: any) {
  selectedScore.value = score;
  detailModal.value?.showModal();
}

function closeDetail() {
  detailModal.value?.close();
  selectedScore.value = null;
}
</script>
