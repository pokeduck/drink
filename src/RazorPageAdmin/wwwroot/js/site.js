document.body.addEventListener('htmx:beforeRequest', function (evt) {

  console.log('beforeRequest');
  showLoading();
});

document.body.addEventListener('htmx:afterRequest', function (evt) {

  console.log('afterRequest');
  hideLoading();
});

let loadingTimer = null;
let lastStartTime = 0;
let isLoadingVisible = false;

function showLoading() {
  const overlay = document.getElementById("global-loading");

  const now = Date.now();
  lastStartTime = now;

  if (!isLoadingVisible) {
    overlay.style.display = "flex";
    isLoadingVisible = true;
  }

  // 若正在 loading，就重設 timer（確保最少 1 秒）
  if (loadingTimer) {
    clearTimeout(loadingTimer);
  }

  // 設置一個新的 hide 計時器，但不會馬上 hide
  loadingTimer = setTimeout(() => {
    tryHideLoading();
  }, 1000);
}

function tryHideLoading() {
  const overlay = document.getElementById("global-loading");

  const elapsed = Date.now() - lastStartTime; // 計算距離最後一次 show 的時間

  if (elapsed < 1000) {
    // 不到 1 秒 → 重新排程
    const remaining = 1000 - elapsed;

    loadingTimer = setTimeout(() => {
      tryHideLoading();
    }, remaining);

    return;
  }

  // 超過 1 秒 → 真正隱藏 overlay
  overlay.style.display = "none";
  isLoadingVisible = false;
  loadingTimer = null;
}

function hideLoading() {
  // hide 並不會立即隱藏，而是交給 tryHideLoading 判斷是否已達 1 秒
  tryHideLoading();
}
