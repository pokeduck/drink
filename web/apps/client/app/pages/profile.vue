<script setup lang="ts">
import { Settings, Shield, Bell, CreditCard, LogOut, ChevronRight } from 'lucide-vue-next'

const { currentUser } = useMockData()

const menuItems = [
  { icon: CreditCard, label: 'Wallet', color: 'text-brand', bg: 'bg-brand/10' },
  { icon: Bell, label: 'Notifications', color: 'text-blue-500', bg: 'bg-blue-50' },
  { icon: Shield, label: 'Security', color: 'text-green-500', bg: 'bg-green-50' },
  { icon: Settings, label: 'Settings', color: 'text-slate-500', bg: 'bg-slate-50' }
]
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-12 md:space-y-16 lg:space-y-20">
    <!-- Profile Header -->
    <div class="flex flex-col md:flex-row items-center md:items-end gap-8 md:gap-12 pt-4 md:pt-8 border-b-8 border-black dark:border-white pb-12">
      <div class="relative">
        <div class="w-40 h-40 md:w-64 md:h-64 brutalist-card p-1 overflow-hidden">
          <img :src="currentUser.avatar" :alt="currentUser.name" class="w-full h-full object-cover">
        </div>
        <div class="absolute -bottom-4 -right-4 w-12 h-12 md:w-16 md:h-16 bg-brand border-4 border-black dark:border-white shadow-brutalist dark:shadow-brutalist-dark-sm flex items-center justify-center text-white">
          <Settings class="w-6 h-6 md:w-8 md:h-8" />
        </div>
      </div>
      <div class="flex-1 text-center md:text-left flex flex-col items-center md:items-start">
        <h2 class="text-5xl md:text-8xl lg:text-9xl italic leading-none truncate">{{ currentUser.name }}</h2>
        <div class="flex flex-col md:flex-row md:items-center gap-3 md:gap-8 mt-6">
          <div class="flex items-center gap-2">
            <span class="w-2 h-2 bg-dark dark:bg-white rounded-full" />
            <p class="text-[10px] md:text-xs font-black uppercase tracking-widest opacity-60">Design Dept</p>
          </div>
          <div class="flex items-center gap-2">
            <span class="w-2 h-2 bg-brand rounded-full" />
            <p class="text-[10px] md:text-xs font-black uppercase tracking-widest text-brand italic">Elite Member</p>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-12 gap-12 lg:gap-16">
      <!-- Stats Sidebar -->
      <aside class="md:col-span-4 space-y-8">
        <h3 class="text-2xl italic border-b-4 border-black dark:border-white inline-block pr-6 pb-1">Stats</h3>
        <div class="grid grid-cols-1 gap-6">
          <div class="brutalist-card p-8 bg-white dark:bg-dark-surface relative overflow-hidden group">
            <div class="absolute -right-4 -top-4 w-16 h-16 bg-brand shadow-brutalist dark:shadow-brutalist-dark opacity-5 rotate-45 group-hover:rotate-90 transition-transform duration-700" />
            <p class="text-[10px] font-black uppercase tracking-widest mb-3 opacity-40">Orders Placed</p>
            <p class="text-6xl font-black italic">12</p>
          </div>
          <div class="brutalist-card p-8 bg-white dark:bg-dark-surface relative overflow-hidden group">
            <div class="absolute -right-4 -top-4 w-16 h-16 bg-green-500 shadow-brutalist dark:shadow-brutalist-dark opacity-5 rotate-45 group-hover:rotate-90 transition-transform duration-700" />
            <p class="text-[10px] font-black uppercase tracking-widest mb-3 opacity-40">Savings</p>
            <p class="text-6xl font-black italic text-green-600 dark:text-green-400">$350</p>
          </div>
        </div>

        <button class="hidden md:flex w-full items-center justify-center gap-4 py-8 brutalist-button hover:bg-black dark:hover:bg-white hover:text-white dark:hover:text-black transition-all shadow-brutalist-soft dark:shadow-brutalist-dark-soft hover:shadow-none border-4">
          <LogOut class="w-6 h-6" />
          <span class="text-xl font-black uppercase tracking-widest">LOGOUT</span>
        </button>
      </aside>

      <!-- Account Menu -->
      <div class="md:col-span-8 space-y-8">
        <h3 class="text-2xl italic border-b-4 border-black dark:border-white inline-block pr-6 pb-1">Account</h3>
        <div class="brutalist-card bg-white dark:bg-dark-surface p-2 divide-y-4 divide-black dark:divide-white/10 border-4">
          <button
            v-for="item in menuItems"
            :key="item.label"
            class="w-full flex items-center justify-between p-8 group text-left transition-all hover:bg-page-bg dark:hover:bg-white/5 active:scale-[0.99]"
          >
            <div class="flex items-center gap-6">
              <div :class="['w-14 h-14 border-4 border-black dark:border-white flex items-center justify-center shadow-brutalist dark:shadow-brutalist-dark', item.color, item.bg]">
                <component :is="item.icon" class="w-8 h-8" />
              </div>
              <div>
                <span class="text-2xl italic font-black group-hover:text-brand transition-colors block leading-none">{{ item.label }}</span>
                <span class="text-[10px] font-black uppercase tracking-widest opacity-30 mt-2 block">Configure Preferences</span>
              </div>
            </div>
            <ChevronRight class="w-8 h-8 text-dark/20 dark:text-white/20 group-hover:text-brand group-hover:translate-x-2 transition-all" />
          </button>
        </div>

        <button class="md:hidden w-full flex items-center justify-center gap-4 py-8 brutalist-button hover:bg-black dark:hover:bg-white hover:text-white dark:hover:text-black transition-all border-4 shadow-none">
          <LogOut class="w-6 h-6" />
          <span class="text-xl font-black italic">LOGOUT</span>
        </button>
      </div>
    </div>
  </div>
</template>
