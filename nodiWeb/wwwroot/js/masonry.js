window.masonry = {
    _observers: [],

    init(selector) {
        // Disconnect all previous observers
        this._observers.forEach(o => o.disconnect());
        this._observers = [];

        const grids = Array.from(document.querySelectorAll(selector));
        if (!grids.length) return;

        // Wait for one animation frame so the browser has finished painting
        // before we measure card heights (avoids getting 0 on first load)
        requestAnimationFrame(() => {
            grids.forEach(grid => {
                this.layout(grid);
                const obs = new ResizeObserver(() => this.layout(grid));
                obs.observe(grid);
                this._observers.push(obs);
            });
        });
    },

    layout(grid) {
        const style     = getComputedStyle(grid);
        const rowHeight = parseInt(style.getPropertyValue('grid-auto-rows'))  || 8;
        const rowGap    = parseInt(style.getPropertyValue('row-gap'))          || 16;
        const items     = Array.from(grid.children);

        // Phase 1: reset ALL spans first (one write pass)
        items.forEach(item => { item.style.gridRowEnd = ''; });

        // Phase 2: read ALL heights in one batch (forces a single reflow)
        const heights = items.map(item => {
            const el = item.firstElementChild || item;
            return el.getBoundingClientRect().height;
        });

        // Phase 3: write ALL spans (one write pass)
        items.forEach((item, i) => {
            const span = Math.ceil((heights[i] + rowGap) / (rowHeight + rowGap));
            item.style.gridRowEnd = `span ${Math.max(span, 1)}`;
        });
    }
};
