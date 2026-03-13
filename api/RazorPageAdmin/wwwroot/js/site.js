document.addEventListener('htmx:beforeRequest', function (evt) {
  console.log('beforeRequest');
  // showLoading();
});

document.addEventListener('htmx:afterRequest', function (evt) {
  const link = evt.target.closest('.app-sidebar a.nav-link');

  // 1. collapse 所有已展開的 treeview
  document.querySelectorAll('.nav-item.menu-open').forEach((item) => {
    item.classList.remove('menu-open');
    const submenu = item.querySelector('.nav-treeview');
    if (submenu) submenu.style.display = 'none';
  });

  // 2. 清除所有 active
  document.querySelectorAll('.app-sidebar a.nav-link.active').forEach((el) => el.classList.remove('active'));

  if (!link) return;

  // 3. 設定目前的 link 為 active
  link.classList.add('active');

  // 4. 讓它的所有父層 treeview 展開
  let parent = link.closest('.nav-item');
  while (parent) {
    const tree = parent.closest('.nav-item');
    if (!tree) break;

    const submenu = tree.querySelector('.nav-treeview');
    if (submenu) {
      tree.classList.add('menu-open');
      submenu.style.display = 'block';
    }

    parent = tree.parentElement.closest('.nav-item');
  }
  console.log('afterRequest');
  // hideLoading();
});

let loadingTimer = null;
let lastStartTime = 0;
let isLoadingVisible = false;

function showLoading() {
  const overlay = document.getElementById('global-loading');

  const now = Date.now();
  lastStartTime = now;

  if (!isLoadingVisible) {
    overlay.style.display = 'flex';
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
  const overlay = document.getElementById('global-loading');

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
  overlay.style.display = 'none';
  isLoadingVisible = false;
  loadingTimer = null;
}

function hideLoading() {
  // hide 並不會立即隱藏，而是交給 tryHideLoading 判斷是否已達 1 秒
  tryHideLoading();
}

document.addEventListener('DOMContentLoaded', function () {
  // 串接 ui:init event
  document.dispatchEvent(new CustomEvent('ui:init', { detail: { root: document } }));
  console.log('set event DOMContentLoaded');

  // 取得目前的 path（例如 /products/index）
  const currentPath = window.location.pathname.toLowerCase();

  // 找到 sidebar 裡面 hx-get 等於該 path 的 <a>
  const link = document.querySelector(`.app-sidebar a.nav-link[hx-get="${currentPath}"]`);

  if (!link) return;

  // 如果是 treeview toggle，那不 active
  if (link.querySelector('.nav-arrow')) return;

  // 1. 清除所有 active
  document.querySelectorAll('.app-sidebar a.nav-link.active').forEach((el) => el.classList.remove('active'));

  // 2. 設定 active
  link.classList.add('active');

  // 3. 展開所有父層 treeview
  let parent = link.closest('.nav-item');
  while (parent) {
    const tree = parent.closest('.nav-item');
    if (!tree) break;

    const submenu = tree.querySelector('.nav-treeview');
    if (submenu) {
      tree.classList.add('menu-open');
      submenu.style.display = 'block';
    }

    parent = tree.parentElement.closest('.nav-item');
  }
});

// 1. 監聽 htmx
document.addEventListener('htmx:afterSwap', (evt) => {
  // 建立一個自訂事件
  console.log('set event htmx:afterSwap');
  const event = new CustomEvent('ui:init', {
    detail: { root: evt.target },
  });
  document.dispatchEvent(event);
});
