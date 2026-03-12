<template>
  <div class="space-y-6">
    <!-- 筛选器 -->
    <div class="card bg-base-100 shadow-lg">
      <div class="card-body">
        <div class="grid md:grid-cols-3 gap-4">
          <div class="form-control">
            <label class="label">
              <span class="label-text">教学楼</span>
            </label>
            <select v-model="selectedBuilding" class="select select-bordered">
              <option v-for="building in buildings" :key="building.id" :value="building.id">
                {{ building.name }}
              </option>
            </select>
          </div>
          <div class="form-control">
            <label class="label">
              <span class="label-text">日期</span>
            </label>
            <input type="date" v-model="selectedDate" class="input input-bordered" />
          </div>
          <div class="form-control">
            <label class="label">
              <span class="label-text">节次</span>
            </label>
            <select v-model="selectedPeriod" class="select select-bordered">
              <option value="all">全部节次</option>
              <option v-for="period in periods" :key="period" :value="period">
                第 {{ period }} 节
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- 结果列表 -->
    <div class="card bg-base-100 shadow-lg overflow-hidden">
      <div class="card-body py-4 border-b border-base-300 flex items-center justify-between">
        <h3 class="card-title text-base">空教室列表</h3>
        <div class="flex items-center gap-3">
          <span v-if="isLoading" class="loading loading-spinner loading-sm text-primary"></span>
          <span class="badge badge-ghost">共 {{ filteredClassrooms.length }} 间</span>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="filteredClassrooms.length === 0" class="p-12 text-center">
        <div class="text-6xl mb-4">🔍</div>
        <h4 class="text-lg font-semibold text-base-content mb-2">没有找到符合条件的教室</h4>
        <p class="text-base-content/50">试试调整筛选条件</p>
      </div>

      <div v-else class="divide-y divide-base-300">
        <div
          v-for="room in filteredClassrooms"
          :key="room.name"
          class="p-4 hover:bg-base-200 transition-all duration-200 flex items-center justify-between group cursor-pointer"
          @click="toggleFavorite(room.name)"
        >
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center text-2xl group-hover:scale-110 transition-transform">
              🏫
            </div>
            <div>
              <div class="font-semibold text-base-content flex items-center gap-2">
                {{ room.name }}
                <span
                  v-if="favorites.includes(room.name)"
                  class="text-yellow-500 text-sm"
                  title="已收藏"
                >
                  ★
                </span>
              </div>
              <div class="text-sm text-base-content/50">{{ room.building }}</div>
            </div>
          </div>
          <div class="flex items-center gap-3">
            <span class="badge badge-success">空闲</span>
            <span class="text-sm text-base-content/50">{{ room.period }}节</span>
            <button
              class="btn btn-ghost btn-xs opacity-0 group-hover:opacity-100 transition-opacity"
              :class="{ 'text-yellow-500': favorites.includes(room.name) }"
              @click.stop="toggleFavorite(room.name)"
            >
              {{ favorites.includes(room.name) ? '已收藏' : '收藏' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 收藏列表 -->
    <div v-if="favorites.length > 0" class="card bg-base-100 shadow-lg overflow-hidden">
      <div class="card-body py-4 border-b border-base-300">
        <h3 class="card-title text-base flex items-center gap-2">
          <span class="text-yellow-500">★</span>
          我的收藏
        </h3>
      </div>
      <div class="divide-y divide-base-300">
        <div
          v-for="roomName in favorites"
          :key="roomName"
          class="p-4 hover:bg-base-200 transition-colors flex items-center justify-between"
        >
          <div class="flex items-center gap-4">
            <div class="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center text-lg">
              ⭐
            </div>
            <span class="font-medium text-base-content">{{ roomName }}</span>
          </div>
          <button class="btn btn-ghost btn-xs text-error" @click="toggleFavorite(roomName)">
            移除
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

const selectedBuilding = ref('all');
const selectedDate = ref('2025-03-12');
const selectedPeriod = ref('all');
const isLoading = ref(false);
const favorites = ref<string[]>([]);

const buildings = [
  { id: 'all', name: '全部教学楼' },
  { id: 'A', name: 'A座教学楼' },
  { id: 'B', name: 'B座教学楼' },
  { id: 'C', name: 'C座教学楼' },
  { id: 'D', name: '实验楼' },
];

const periods = [1, 2, 3, 4, 5, 6, 7, 8];

const classrooms = [
  { name: 'A101', building: 'A座教学楼', buildingId: 'A', period: '1-2', startPeriod: 1, endPeriod: 2 },
  { name: 'A102', building: 'A座教学楼', buildingId: 'A', period: '3-4', startPeriod: 3, endPeriod: 4 },
  { name: 'A201', building: 'A座教学楼', buildingId: 'A', period: '5-6', startPeriod: 5, endPeriod: 6 },
  { name: 'A202', building: 'A座教学楼', buildingId: 'A', period: '7-8', startPeriod: 7, endPeriod: 8 },
  { name: 'A301', building: 'A座教学楼', buildingId: 'A', period: '1-2', startPeriod: 1, endPeriod: 2 },
  { name: 'B101', building: 'B座教学楼', buildingId: 'B', period: '1-2', startPeriod: 1, endPeriod: 2 },
  { name: 'B102', building: 'B座教学楼', buildingId: 'B', period: '3-4', startPeriod: 3, endPeriod: 4 },
  { name: 'B201', building: 'B座教学楼', buildingId: 'B', period: '7-8', startPeriod: 7, endPeriod: 8 },
  { name: 'B301', building: 'B座教学楼', buildingId: 'B', period: '5-6', startPeriod: 5, endPeriod: 6 },
  { name: 'C101', building: 'C座教学楼', buildingId: 'C', period: '1-2', startPeriod: 1, endPeriod: 2 },
  { name: 'C102', building: 'C座教学楼', buildingId: 'C', period: '5-6', startPeriod: 5, endPeriod: 6 },
  { name: 'C201', building: 'C座教学楼', buildingId: 'C', period: '3-4', startPeriod: 3, endPeriod: 4 },
  { name: 'D301', building: '实验楼', buildingId: 'D', period: '3-4', startPeriod: 3, endPeriod: 4 },
  { name: 'D302', building: '实验楼', buildingId: 'D', period: '7-8', startPeriod: 7, endPeriod: 8 },
  { name: 'D401', building: '实验楼', buildingId: 'D', period: '1-2', startPeriod: 1, endPeriod: 2 },
];

// 模拟加载效果
watch([selectedBuilding, selectedDate, selectedPeriod], () => {
  isLoading.value = true;
  setTimeout(() => {
    isLoading.value = false;
  }, 300);
});

const filteredClassrooms = computed(() => {
  return classrooms.filter(room => {
    // 教学楼筛选
    if (selectedBuilding.value !== 'all' && room.buildingId !== selectedBuilding.value) {
      return false;
    }
    // 节次筛选
    if (selectedPeriod.value !== 'all') {
      const period = selectedPeriod.value as number;
      if (period < room.startPeriod || period > room.endPeriod) {
        return false;
      }
    }
    return true;
  });
});

function toggleFavorite(roomName: string) {
  const index = favorites.value.indexOf(roomName);
  if (index > -1) {
    favorites.value.splice(index, 1);
  } else {
    favorites.value.push(roomName);
  }
}
</script>
