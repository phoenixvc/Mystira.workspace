(function() {
  const STORAGE_KEY = 'mystira_three_panel_widths';
  function clamp(val, min, max) { return Math.max(min, Math.min(max, val)); }

  function setWidths(leftEl, centerEl, rightEl, left, center, right) {
    leftEl.style.width = left + '%';
    centerEl.style.width = center + '%';
    rightEl.style.width = right + '%';
  }

  function save(left, center, right) {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ left, center, right }));
    } catch {}
  }

  function load() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const obj = JSON.parse(raw);
      if (typeof obj.left === 'number' && typeof obj.center === 'number' && typeof obj.right === 'number') return obj;
    } catch {}
    return null;
  }

  window.mystiraPanels = {
    init: function(config) {
      const container = document.getElementById(config.containerId || 'panel-container');
      const leftEl = document.getElementById(config.leftId || 'panel-left');
      const centerEl = document.getElementById(config.centerId || 'panel-center');
      const rightEl = document.getElementById(config.rightId || 'panel-right');
      const leftResizer = document.getElementById(config.leftResizerId || 'resizer-left');
      const rightResizer = document.getElementById(config.rightResizerId || 'resizer-right');
      if (!container || !leftEl || !centerEl || !rightEl || !leftResizer || !rightResizer) return;

      const mins = Object.assign({ left: 15, center: 25, right: 20 }, config.mins || {});
      const stored = load();
      let left = stored?.left ?? 30;
      let center = stored?.center ?? 40;
      let right = stored?.right ?? 30;
      // Normalize to sum 100
      const total = left + center + right;
      if (total !== 100) {
        left = left / total * 100;
        center = center / total * 100;
        right = 100 - left - center;
      }
      setWidths(leftEl, centerEl, rightEl, left, center, right);

      function onMouseMoveLeft(e) {
        const rect = container.getBoundingClientRect();
        const dxPercent = (e.clientX - startX) / rect.width * 100;
        let newLeft = startLeft + dxPercent;
        let newCenter = startCenter - dxPercent;
        // Clamp to mins
        newLeft = clamp(newLeft, mins.left, 100 - mins.center - right);
        newCenter = clamp(newCenter, mins.center, 100 - newLeft - right);
        // Recompute based on clamped center
        const adjDx = (startCenter - newCenter);
        newLeft = startLeft + adjDx;
        setWidths(leftEl, centerEl, rightEl, newLeft, newCenter, right);
      }

      function onMouseMoveRight(e) {
        const rect = container.getBoundingClientRect();
        const dxPercent = (startX - e.clientX) / rect.width * 100; // dragging left increases center
        let newRight = startRight + dxPercent;
        let newCenter = startCenter - dxPercent;
        newRight = clamp(newRight, mins.right, 100 - mins.center - left);
        newCenter = clamp(newCenter, mins.center, 100 - left - newRight);
        const adjDx = (startCenter - newCenter);
        newRight = startRight + adjDx;
        setWidths(leftEl, centerEl, rightEl, left, newCenter, newRight);
      }

      let startX = 0, startLeft = 0, startCenter = 0, startRight = 0;
      let moveHandler = null, upHandler = null;

      function attachMove(type) {
        // snapshot starting values
        const rect = container.getBoundingClientRect();
        startLeft = parseFloat(leftEl.style.width) || left;
        startCenter = parseFloat(centerEl.style.width) || center;
        startRight = parseFloat(rightEl.style.width) || right;

        moveHandler = (ev) => {
          if (type === 'left') onMouseMoveLeft(ev); else onMouseMoveRight(ev);
        };
        upHandler = () => {
          document.removeEventListener('mousemove', moveHandler);
          document.removeEventListener('mouseup', upHandler);
          // Persist widths
          const wl = parseFloat(leftEl.style.width) || left;
          const wc = parseFloat(centerEl.style.width) || center;
          const wr = parseFloat(rightEl.style.width) || right;
          save(wl, wc, wr);
        };
        document.addEventListener('mousemove', moveHandler);
        document.addEventListener('mouseup', upHandler);
      }

      leftResizer.addEventListener('mousedown', function(e) {
        e.preventDefault();
        startX = e.clientX;
        attachMove('left');
      });
      rightResizer.addEventListener('mousedown', function(e) {
        e.preventDefault();
        startX = e.clientX;
        attachMove('right');
      });
    }
  };
})();
