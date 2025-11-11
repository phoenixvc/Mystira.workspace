(function () {
    const clamp = (value, min, max) => {
        const number = Number(value);
        if (Number.isNaN(number)) {
            return min;
        }
        return Math.min(Math.max(number, min), max);
    };

    const scrollToMatch = (container, matchIndex) => {
        if (!container || matchIndex === null || matchIndex === undefined) {
            return;
        }

        const index = Number(matchIndex);
        if (Number.isNaN(index) || index < 0) {
            return;
        }

        const target = container.querySelector(`[data-match-index="${index}"]`);
        if (!target || typeof target.scrollIntoView !== 'function') {
            return;
        }

        target.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'nearest' });
    };

    const selectTextareaRange = (textarea, start, length) => {
        if (!textarea) {
            return;
        }

        window.requestAnimationFrame(() => {
            const value = textarea.value ?? '';
            const safeStart = clamp(start, 0, value.length);
            const safeLength = Math.max(0, Number(length) || 0);
            const safeEnd = clamp(safeStart + safeLength, safeStart, value.length);

            if (typeof textarea.setSelectionRange === 'function') {
                try {
                    textarea.setSelectionRange(safeStart, safeEnd);
                } catch (err) {
                    console.warn('Mystira YAML preview selection error', err);
                }
            }

            if (typeof textarea.focus === 'function') {
                textarea.focus({ preventScroll: true });
            }

            const computed = window.getComputedStyle(textarea);
            const lineHeight = parseInt(computed.lineHeight, 10) || 16;
            const preText = value.slice(0, safeStart);
            const lineBreaks = (preText.match(/\n/g) || []).length;
            const approxTop = lineBreaks * lineHeight;
            const targetTop = Math.max(approxTop - textarea.clientHeight / 2, 0);
            textarea.scrollTop = targetTop;
        });
    };

    window.mystiraYamlPreview = window.mystiraYamlPreview || {};
    window.mystiraYamlPreview.scrollToMatch = scrollToMatch;
    window.mystiraYamlPreview.selectTextareaRange = selectTextareaRange;
})();
