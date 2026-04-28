<script setup lang="ts">
import { OrderStatus } from '~/composables/useMockData'

const { groups } = useMockData()

const activeGroups = computed(() =>
  groups.filter(g => g.status !== OrderStatus.COMPLETED && g.status !== OrderStatus.CANCELLED)
)

const pastGroups = computed(() =>
  groups.filter(g => g.status === OrderStatus.COMPLETED || g.status === OrderStatus.CANCELLED)
)
</script>

<template>
  <div class="space-y-16">
    <section>
      <SectionHeader title="Active Groups">
        <template #right>
          <span class="hidden md:block text-xs font-black uppercase tracking-widest opacity-40 dark:text-dark-text/40">
            Showing {{ activeGroups.length }} open buys
          </span>
        </template>
      </SectionHeader>

      <div v-if="activeGroups.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        <div
          v-for="(group, index) in activeGroups"
          :key="group.id"
          class="animate-fade-in"
          :style="{ animationDelay: `${index * 50}ms` }"
        >
          <GroupCard :group="group" />
        </div>
      </div>

      <EmptyState
        v-else
        title="NO ACTIVE BUYS"
        subtitle="Wait for someone to start, or lead the way!"
      />
    </section>

    <section v-if="pastGroups.length > 0">
      <SectionHeader title="History" muted />
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 opacity-60 grayscale">
        <div
          v-for="(group, index) in pastGroups"
          :key="group.id"
          class="animate-fade-in"
          :style="{ animationDelay: `${index * 50}ms` }"
        >
          <GroupCard :group="group" />
        </div>
      </div>
    </section>
  </div>
</template>
