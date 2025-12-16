document.addEventListener('ui:init', (e) => {
  console.log('ui:init-apexcharts');
  const root = e.target || document;
  root.querySelectorAll('[data-ui="apexchart"]').forEach((el) => {
    if (el.dataset.inited) return;

    const options = JSON.parse(el.dataset.chartOptions || '{}');
    new ApexCharts(el, options).render();

    el.dataset.inited = 'true';
  });
});
