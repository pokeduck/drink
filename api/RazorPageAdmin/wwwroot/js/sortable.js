document.addEventListener('ui:init', (e) => {
  console.log('ui:init-sortable');
  const root = e.target || document;
  root.querySelectorAll('[data-ui="sortable"]').forEach((el) => {
    if (el.dataset.inited) return;

    new Sortable(el, {
      group: 'shared',
      handle: '.card-header',
    });

    el.querySelectorAll('.card-header').forEach((header) => {
      header.style.cursor = 'move';
    });

    el.dataset.inited = 'true';
  });
});
