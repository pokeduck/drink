import { hash } from "ohash";

export const useApi = () => {
  const config = useRuntimeConfig();
  const token = useCookie("auth_token"); // 假設你把 JWT 存在 Cookie

  const apiFetch = $fetch.create({
    // 1. 設定 Base URL
    baseURL: config.public.apiBase,

    // 2. 請求攔截器 (Request Interceptor)
    async onRequest({ options }) {
      // 確保 headers 存在且是 Headers 實例
      const headers = new Headers(options.headers);

      if (token.value) {
        headers.set("Authorization", `Bearer ${token.value}`);
      }

      options.headers = headers;
    },

    // 3. 錯誤處理攔截器 (Response Error Interceptor)
    async onResponseError({ response }) {
      if (response.status === 401) {
        // Token 過期或無效，跳轉回登入頁
        await navigateTo("/login");
      } else {
        // TODO: 串接 Element Plus 的 Message 提示錯誤訊息
        console.error("API Error:", response._data?.message || "未知錯誤");
      }
    },
  });

  // 回傳封裝好的請求方法
  return {
    get: <T>(url: string, opts?: any) => apiFetch<T>(url, { method: "GET", ...opts }),
    post: <T>(url: string, body?: any, opts?: any) => apiFetch<T>(url, { method: "POST", body, ...opts }),
    put: <T>(url: string, body?: any, opts?: any) => apiFetch<T>(url, { method: "PUT", body, ...opts }),
    delete: <T>(url: string, opts?: any) => apiFetch<T>(url, { method: "DELETE", ...opts }),
  };
};
