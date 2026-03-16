export enum DrinkStatus {
  PREPARING = "PREPARING",
  READY = "READY",
  PICKED_UP = "PICKED_UP",
}

export interface DrinkOrder {
  id: number;
  name: string;
  status: DrinkStatus;
  price: number;
  createdAt: string;
}
