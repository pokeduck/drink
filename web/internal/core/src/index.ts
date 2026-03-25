import { DrinkStatus } from "@app/models";

export const getStatusLabel = (status: DrinkStatus): string => {
  const labels: Record<DrinkStatus, string> = {
    [DrinkStatus.PREPARING]: "飲品製作中 ☕",
    [DrinkStatus.READY]: "請來取餐囉 🏃‍♂️",
    [DrinkStatus.PICKED_UP]: "已取餐，祝您好運 🍀",
  };
  return labels[status];
};

export const formatPrice = (price: number) => `NT$ ${price}`;

export const convertDate = (date: string) => new Date(date).toLocaleDateString();

export const testFunction = () => {
  console.log("This is a test function.");
};
