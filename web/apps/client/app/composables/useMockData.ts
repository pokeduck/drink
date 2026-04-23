export interface User {
  id: string
  name: string
  avatar: string
}

export enum OrderStatus {
  OPEN = 'OPEN',
  ORDERED = 'ORDERED',
  ARRIVING = 'ARRIVING',
  READY = 'READY',
  COMPLETED = 'COMPLETED',
  CANCELLED = 'CANCELLED'
}

export interface Drink {
  id: string
  name: string
  price: number
  category: string
  description?: string
}

export interface GroupOrder {
  id: string
  hostId: string
  storeName: string
  storeLogo: string
  deadline: string
  status: OrderStatus
  description: string
  items: MemberOrder[]
}

export interface MemberOrder {
  id: string
  userId: string
  userName: string
  drinkId: string
  drinkName: string
  specifications: string
  price: number
  paid: boolean
}

export interface Store {
  id: string
  name: string
  logo: string
  menu: Drink[]
}

const MOCK_USERS: User[] = [
  { id: 'u1', name: '阿明', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Felix' },
  { id: 'u2', name: '小美', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Anya' },
  { id: 'u3', name: '大強', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Caleb' }
]

const CURRENT_USER = MOCK_USERS[0]

const MOCK_STORES: Store[] = [
  {
    id: 's1',
    name: '五十嵐',
    logo: 'https://api.dicebear.com/7.x/initials/svg?seed=50L&backgroundColor=facc15',
    menu: [
      { id: 'd1', name: '珍珠奶茶', price: 50, category: '奶茶' },
      { id: 'd2', name: '四季春茶', price: 30, category: '純茶' },
      { id: 'd3', name: '冰淇淋紅茶', price: 45, category: '茶' },
      { id: 'd4', name: '燕麥拿鐵', price: 65, category: '咖啡/拿鐵' }
    ]
  },
  {
    id: 's2',
    name: '迷客夏',
    logo: 'https://api.dicebear.com/7.x/initials/svg?seed=MK&backgroundColor=22c55e',
    menu: [
      { id: 'd10', name: '珍珠鮮奶', price: 70, category: '鮮奶' },
      { id: 'd11', name: '大甲芋頭鮮奶', price: 80, category: '鮮奶' },
      { id: 'd12', name: '青茶', price: 35, category: '純茶' }
    ]
  },
  {
    id: 's3',
    name: '可不可熟成紅茶',
    logo: 'https://api.dicebear.com/7.x/initials/svg?seed=KBK&backgroundColor=0c4a6e',
    menu: [
      { id: 'd20', name: '熟成紅茶', price: 30, category: '紅茶' },
      { id: 'd21', name: '胭脂紅茶', price: 40, category: '紅茶' },
      { id: 'd22', name: '白玉歐蕾', price: 60, category: '奶茶' }
    ]
  },
  ...Array.from({ length: 49 }, (_, i) => ({
    id: `s-extra-${i}`,
    name: `手搖飲店 ${i + 4}`,
    logo: `https://api.dicebear.com/7.x/initials/svg?seed=D${i + 4}&backgroundColor=cbd5e1`,
    menu: [
      { id: `d-e-${i}-1`, name: '經典奶茶', price: 45, category: '奶茶' },
      { id: `d-e-${i}-2`, name: '招牌綠茶', price: 30, category: '純茶' }
    ]
  }))
]

const MOCK_GROUPS: GroupOrder[] = [
  {
    id: 'g1',
    hostId: 'u1',
    storeName: '五十嵐',
    storeLogo: 'https://api.dicebear.com/7.x/initials/svg?seed=50L&backgroundColor=facc15',
    deadline: new Date(Date.now() + 3600000).toISOString(),
    status: OrderStatus.OPEN,
    description: '下午茶時間！大家快點喔～ 15:00 準時截單',
    items: [
      { id: 'o1', userId: 'u2', userName: '小美', drinkId: 'd1', drinkName: '珍珠奶茶', specifications: '大杯, 微糖微冰', price: 50, paid: true },
      { id: 'o2', userId: 'u3', userName: '大強', drinkId: 'd2', drinkName: '四季春茶', specifications: '中杯, 無糖去冰', price: 30, paid: false },
      { id: 'o3', userId: 'u1', userName: '阿明', drinkId: 'd1', drinkName: '珍珠奶茶', specifications: '大杯, 半糖少冰', price: 50, paid: true },
      { id: 'o4', userId: 'u1', userName: '阿明', drinkId: 'd4', drinkName: '燕麥拿鐵', specifications: '大杯, 無糖去冰', price: 65, paid: false }
    ]
  },
  {
    id: 'g2',
    hostId: 'u2',
    storeName: '迷客夏',
    storeLogo: 'https://api.dicebear.com/7.x/initials/svg?seed=MK&backgroundColor=22c55e',
    deadline: new Date(Date.now() + 7200000).toISOString(),
    status: OrderStatus.READY,
    description: '迷客夏鮮奶系列來囉！',
    items: [
      { id: 'o5', userId: 'u1', userName: '阿明', drinkId: 'd10', drinkName: '珍珠鮮奶', specifications: '大杯, 微糖去冰', price: 70, paid: true },
      { id: 'o6', userId: 'u3', userName: '大強', drinkId: 'd12', drinkName: '青茶', specifications: '大杯, 無糖去冰', price: 35, paid: true }
    ]
  },
  {
    id: 'g3',
    hostId: 'u3',
    storeName: '可不可熟成紅茶',
    storeLogo: 'https://api.dicebear.com/7.x/initials/svg?seed=KBK&backgroundColor=0c4a6e',
    deadline: new Date(Date.now() - 3600000).toISOString(),
    status: OrderStatus.COMPLETED,
    description: '早上的紅茶，今天辛苦了。',
    items: [
      { id: 'o7', userId: 'u1', userName: '阿明', drinkId: 'd20', drinkName: '熟成紅茶', specifications: '大杯, 微糖去冰', price: 30, paid: true },
      { id: 'o8', userId: 'u1', userName: '阿明', drinkId: 'd22', drinkName: '白玉歐蕾', specifications: '大杯, 半糖少冰', price: 60, paid: true }
    ]
  },
  {
    id: 'g4',
    hostId: 'u2',
    storeName: '五十嵐',
    storeLogo: 'https://api.dicebear.com/7.x/initials/svg?seed=50L&backgroundColor=facc15',
    deadline: new Date(Date.now() - 172800000).toISOString(),
    status: OrderStatus.COMPLETED,
    description: '上週五的補給。',
    items: [
      { id: 'o9', userId: 'u1', userName: '阿明', drinkId: 'd1', drinkName: '珍珠奶茶', specifications: '大杯, 微糖', price: 50, paid: true },
      { id: 'o10', userId: 'u1', userName: '阿明', drinkId: 'd3', drinkName: '冰淇淋紅茶', specifications: '大杯, 正常', price: 45, paid: true }
    ]
  },
  {
    id: 'g5',
    hostId: 'u1',
    storeName: '迷客夏',
    storeLogo: 'https://api.dicebear.com/7.x/initials/svg?seed=MK&backgroundColor=22c55e',
    deadline: new Date(Date.now() - 432000000).toISOString(),
    status: OrderStatus.COMPLETED,
    description: '好喝的鮮奶。',
    items: [
      { id: 'o11', userId: 'u1', userName: '阿明', drinkId: 'd11', drinkName: '大甲芋頭鮮奶', specifications: '大杯, 微糖', price: 80, paid: true },
      { id: 'o12', userId: 'u1', userName: '阿明', drinkId: 'd10', drinkName: '珍珠鮮奶', specifications: '大杯, 無糖', price: 70, paid: true },
      { id: 'o13', userId: 'u1', userName: '阿明', drinkId: 'd12', drinkName: '青茶', specifications: '大杯, 無糖', price: 35, paid: true }
    ]
  }
]

export function useMockData() {
  return {
    users: MOCK_USERS,
    currentUser: CURRENT_USER,
    stores: MOCK_STORES,
    groups: MOCK_GROUPS
  }
}
