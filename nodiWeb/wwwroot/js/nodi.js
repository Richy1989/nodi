window.nodi = {
    setTheme(dark) {
        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
    },
    focusById(id) {
        document.getElementById(id)?.focus();
    }
};
