(() => {
  const key = 'lifeos-theme';
  const root = document.documentElement;
  const systemDark = () => window.matchMedia('(prefers-color-scheme: dark)').matches;
  const resolve = () => localStorage.getItem(key) || (systemDark() ? 'dark' : 'light');
  const apply = theme => { root.dataset.theme = theme; root.style.colorScheme = theme; };
  apply(resolve());
  window.lifeosTheme = { toggle: () => { const next = root.dataset.theme === 'dark' ? 'light' : 'dark'; localStorage.setItem(key, next); apply(next); }, clear: () => { localStorage.removeItem(key); apply(resolve()); } };
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => { if (!localStorage.getItem(key)) apply(resolve()); });
})();
